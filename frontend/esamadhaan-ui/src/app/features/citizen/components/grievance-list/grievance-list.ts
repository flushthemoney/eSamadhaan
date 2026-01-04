import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
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
export class GrievanceListComponent implements OnInit {
  displayedColumns: string[] = ['grievanceNumber', 'categoryName', 'departmentName', 'status', 'createdAt', 'actions'];
  grievances: GrievanceListDto[] = [];
  filteredGrievances: GrievanceListDto[] = [];
  isLoading = false;
  selectedStatus: number | null = null;
  pageSize = 25;
  pageIndex = 0;
  GrievanceStatus = GrievanceStatus;

  constructor(private grievanceService: GrievanceService) {}

  ngOnInit(): void {
    this.loadGrievances();
  }

  loadGrievances(): void {
    this.isLoading = true;
    this.grievanceService.getMyGrievances(this.selectedStatus || undefined).subscribe({
      next: (data) => {
        this.grievances = data;
        this.applyPagination();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  onStatusFilterChange(): void {
    this.pageIndex = 0;
    this.loadGrievances();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    this.filteredGrievances = this.grievances.slice(start, end);
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
