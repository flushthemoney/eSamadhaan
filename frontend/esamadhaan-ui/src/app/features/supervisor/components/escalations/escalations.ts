import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';

@Component({
  selector: 'app-escalations',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
  ],
  templateUrl: './escalations.html',
  styleUrl: './escalations.scss',
})
export class EscalationsComponent implements OnInit {
  displayedColumns: string[] = ['grievanceNumber', 'departmentName', 'categoryName', 'status', 'daysSinceSubmission', 'actions'];
  escalations: any[] = [];
  filteredEscalations: any[] = [];
  isLoading = false;
  pageSize = 25;
  pageIndex = 0;

  constructor(private grievanceService: GrievanceService) {}

  ngOnInit(): void {
    this.loadEscalations();
  }

  loadEscalations(): void {
    this.isLoading = true;
    this.grievanceService.getEscalatedGrievances().subscribe({
      next: (data) => {
        this.escalations = data;
        this.applyPagination();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    this.filteredEscalations = this.escalations.slice(start, end);
  }
}
