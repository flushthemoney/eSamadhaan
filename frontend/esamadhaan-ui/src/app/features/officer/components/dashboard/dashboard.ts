import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatGridListModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatPaginatorModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    DatePipe,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['grievanceNumber', 'status', 'createdAt', 'actions'];
  dashboardData: any = null;
  dataSource = new MatTableDataSource<any>([]);
  isLoading = false;
  resolutionRate: number = 0;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(private grievanceService: GrievanceService, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.grievanceService.getOfficerDashboard().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.dashboardData = data;
          if (data.recentGrievances) {
            this.dataSource.data = data.recentGrievances;
          }
          this.updateResolutionRate();
          this.isLoading = false;
          this.cdr.markForCheck();
          // Connect paginator after view updates
          this.connectPaginatorWithRetry();
        }, 0);
      },
      error: () => {
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        }, 0);
      },
    });
  }

  private updateResolutionRate(): void {
    if (
      !this.dashboardData ||
      !this.dashboardData.totalGrievances ||
      this.dashboardData.totalGrievances === 0
    ) {
      this.resolutionRate = 0;
      return;
    }
    const resolved = this.dashboardData.resolvedCount || 0;
    this.resolutionRate = Math.round((resolved / this.dashboardData.totalGrievances) * 100);
  }
  
  ngAfterViewInit(): void {
    // Try to connect paginator when view is initialized
    this.connectPaginatorWithRetry();
  }

  private connectPaginatorWithRetry(attempts = 0): void {
    const maxAttempts = 10;
    if (attempts >= maxAttempts) {
      return;
    }

    setTimeout(() => {
      if (this.paginator && this.dataSource) {
        this.dataSource.paginator = this.paginator;
        this.cdr.markForCheck();
      } else if (attempts < maxAttempts) {
        // Retry if paginator not available yet (might be conditionally rendered)
        this.connectPaginatorWithRetry(attempts + 1);
      }
    }, 50);
  }


  formatDate(dateString: string): Date {
    // Ensure proper UTC parsing - backend sends dates in UTC
    let dateStr = dateString.trim();

    // Check if it already has timezone info (Z for UTC or +/- offset)
    const hasTimezone = dateStr.endsWith('Z') || /[+-]\d{2}:?\d{2}$/.test(dateStr);

    if (!hasTimezone && dateStr.includes('T')) {
      // ISO format without timezone - append Z to indicate UTC
      // Handle milliseconds if present
      if (dateStr.includes('.')) {
        const parts = dateStr.split('.');
        dateStr = parts[0] + 'Z';
      } else {
        dateStr = dateStr + 'Z';
      }
    }

    return new Date(dateStr);
  }
}
