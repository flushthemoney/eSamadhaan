import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceStatus } from '../../../../models/common';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatButtonModule,
    MatGridListModule,
    MatIconModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    DatePipe,
  ],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class AdminDashboardComponent implements OnInit {
  dashboardData: any = null;
  isLoading = false;
  topCategoriesList: Array<{ name: string; count: number }> = [];

  constructor(
    private grievanceService: GrievanceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.isLoading = true;
    this.grievanceService.getSupervisorDashboard().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.dashboardData = data;
          this.updateTopCategories();
          this.isLoading = false;
          this.cdr.markForCheck();
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

  private updateTopCategories(): void {
    if (!this.dashboardData?.topCategories) {
      this.topCategoriesList = [];
      return;
    }
    this.topCategoriesList = Object.entries(this.dashboardData.topCategories)
      .map(([name, count]) => ({ name, count: count as number }))
      .sort((a, b) => b.count - a.count);
  }

  formatResolutionTime(days: number | null | undefined): string {
    if (days == null || days === undefined) {
      return '0d';
    }
    if (days < 1) {
      return `${(days * 24).toFixed(1)}h`;
    }
    return `${days.toFixed(2)}d`;
  }

  formatDate(dateString: string): Date {
    return new Date(dateString);
  }

  getStatusFromString(status: string): GrievanceStatus {
    const statusMap: Record<string, GrievanceStatus> = {
      'Submitted': GrievanceStatus.Submitted,
      'Assigned': GrievanceStatus.Assigned,
      'InReview': GrievanceStatus.InReview,
      'Resolved': GrievanceStatus.Resolved,
      'Closed': GrievanceStatus.Closed,
    };
    return statusMap[status] || GrievanceStatus.Submitted;
  }

  hasTopCategories(): boolean {
    return this.topCategoriesList.length > 0;
  }

  getTopCategories(): Array<{ name: string; count: number }> {
    return this.topCategoriesList;
  }
}
