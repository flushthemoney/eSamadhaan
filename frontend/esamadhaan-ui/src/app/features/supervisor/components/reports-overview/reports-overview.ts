import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';

import { ReportService } from '../../../../services/report.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceStatus } from '../../../../models/common';

@Component({
  selector: 'app-reports-overview',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTableModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
  ],
  templateUrl: './reports-overview.html',
  styleUrl: './reports-overview.scss',
})
export class ReportsOverviewComponent implements OnInit {
  statusReport: any[] = [];
  isLoading = false;
  displayedColumns: string[] = ['status', 'count', 'percentage'];
  GrievanceStatus = GrievanceStatus;

  constructor(
    private reportService: ReportService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadStatusReport();
  }

  loadStatusReport(): void {
    this.isLoading = true;
    this.reportService.getStatusReport().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.statusReport = data;
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
