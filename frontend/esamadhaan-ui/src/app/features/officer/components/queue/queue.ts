import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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
import { FormsModule } from '@angular/forms';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceStatus } from '../../../../models/common';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-queue',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './queue.html',
  styleUrl: './queue.scss',
})
export class QueueComponent implements OnInit {
  displayedColumns: string[] = ['grievanceNumber', 'categoryName', 'status', 'createdAt', 'actions'];
  grievances: any[] = [];
  filteredGrievances: any[] = [];
  isLoading = false;
  selectedStatus: number | null = null;
  selectedCategoryId: number | null = null;
  pageSize = 25;
  pageIndex = 0;
  GrievanceStatus = GrievanceStatus;

  constructor(
    private grievanceService: GrievanceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadQueue();
  }

  loadQueue(): void {
    this.isLoading = true;
    const filters: any = {};
    if (this.selectedStatus) filters.status = this.selectedStatus;
    if (this.selectedCategoryId) filters.categoryId = this.selectedCategoryId;

    this.grievanceService.getMyQueue(filters).subscribe({
      next: (data) => {
        setTimeout(() => {
          this.grievances = data;
          this.applyPagination();
          this.isLoading = false;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        }, 0);
      },
    });
  }

  onFilterChange(): void {
    this.pageIndex = 0;
    this.loadQueue();
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
}
