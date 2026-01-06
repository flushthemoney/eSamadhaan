import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceTimelineComponent } from '../../../../shared/components/grievance-timeline/grievance-timeline';
import { GrievanceListDto } from '../../../../models/grievance';
import { GrievanceStatus } from '../../../../models/common';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatTableModule,
    MatPaginatorModule,
    MatIconModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    GrievanceTimelineComponent,
    RelativeTimePipe,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['grievanceNumber', 'categoryName', 'status', 'createdAt', 'actions'];
  grievances: GrievanceListDto[] = [];
  dataSource = new MatTableDataSource<GrievanceListDto>([]);
  isLoading = false;
  GrievanceStatus = GrievanceStatus;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private grievanceService: GrievanceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadGrievances();
  }

  loadGrievances(): void {
    this.isLoading = true;
    this.grievanceService.getMyGrievances().subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          // Sort by createdAt ascending (oldest first)
          const sortedData = data.sort((a, b) => {
            const dateA = new Date(a.createdAt).getTime();
            const dateB = new Date(b.createdAt).getTime();
            return dateA - dateB; // Ascending order (oldest first)
          });
          this.grievances = sortedData;
          this.dataSource.data = sortedData;
          this.isLoading = false;
          this.cdr.markForCheck();
          // Connect paginator after view updates
          this.connectPaginatorWithRetry();
        }, 0);
      },
      error: () => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        }, 0);
      },
    });
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
}
