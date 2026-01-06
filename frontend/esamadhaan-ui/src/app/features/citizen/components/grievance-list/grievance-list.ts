import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { FormsModule } from '@angular/forms';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceListDto } from '../../../../models/grievance';
import { GrievanceStatus } from '../../../../models/common';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-grievance-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    FormsModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './grievance-list.html',
  styleUrl: './grievance-list.scss',
})
export class GrievanceListComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['grievanceNumber', 'categoryName', 'departmentName', 'status', 'createdAt', 'actions'];
  grievances: GrievanceListDto[] = [];
  dataSource = new MatTableDataSource<GrievanceListDto>([]);
  isLoading = false;
  selectedStatus: number | null = null;
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
    this.cdr.markForCheck();
    this.grievanceService.getMyGrievances(this.selectedStatus || undefined).subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.grievances = data.sort((a, b) => {
            const dateA = new Date(a.createdAt).getTime();
            const dateB = new Date(b.createdAt).getTime();
            return dateB - dateA; // Newest first
          });
          this.dataSource.data = this.grievances;
          this.isLoading = false;
          this.cdr.detectChanges();
          // Connect paginator after view updates - use multiple attempts to ensure it's available
          this.connectPaginatorWithRetry();
        }, 0);
      },
      error: () => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
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
        this.cdr.detectChanges();
      } else if (attempts < maxAttempts) {
        // Retry if paginator not available yet (might be conditionally rendered)
        this.connectPaginatorWithRetry(attempts + 1);
      }
    }, 50);
  }

  onStatusFilterChange(): void {
    this.loadGrievances();
  }

  getStatusLabel(status: GrievanceStatus): string {
    const labels: Record<GrievanceStatus, string> = {
      [GrievanceStatus.Submitted]: 'Submitted',
      [GrievanceStatus.Assigned]: 'Assigned',
      [GrievanceStatus.InReview]: 'In Review',
      [GrievanceStatus.Resolved]: 'Resolved',
      [GrievanceStatus.Closed]: 'Closed',
    };
    return labels[status] || 'Unknown';
  }
}
