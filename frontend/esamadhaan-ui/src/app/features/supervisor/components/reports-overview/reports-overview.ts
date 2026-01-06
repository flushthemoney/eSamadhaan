import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { ReportService } from '../../../../services/report.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatus } from '../../../../models/common';

@Component({
  selector: 'app-reports-overview',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
  ],
  templateUrl: './reports-overview.html',
  styleUrl: './reports-overview.scss',
})
export class ReportsOverviewComponent implements OnInit {
  statusReport: any[] = [];
  isLoading = false;
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
      next: (data: any) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          // Backend now returns camelCase, no normalization needed
          this.statusReport = data.statusBreakdown || [];
          this.isLoading = false;
          this.cdr.markForCheck();
        }, 0);
      },
      error: () => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        }, 0);
      },
    });
  }

  getPieChartData(): any[] {
    if (!this.statusReport || this.statusReport.length === 0) return [];
    return this.statusReport.map((item: any) => ({
      status: item.status,
      count: item.count,
      percentage: item.percentage,
      label: this.getStatusLabel(item.status)
    }));
  }

  getTotalCount(): number {
    return this.statusReport.reduce((sum: number, item: any) => sum + item.count, 0);
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

  getStatusColor(status: GrievanceStatus): string {
    const colors: Record<GrievanceStatus, string> = {
      [GrievanceStatus.Submitted]: '#2196F3', // Blue
      [GrievanceStatus.Assigned]: '#FF9800', // Orange
      [GrievanceStatus.InReview]: '#9C27B0', // Purple
      [GrievanceStatus.Resolved]: '#4CAF50', // Green
      [GrievanceStatus.Closed]: '#757575', // Grey
    };
    return colors[status] || '#9E9E9E';
  }

  getPieChartSegments(): any[] {
    if (!this.statusReport || this.statusReport.length === 0) return [];
    
    const total = this.getTotalCount();
    if (total === 0) return [];

    const centerX = 100;
    const centerY = 100;
    const radius = 80;
    let currentAngle = -90; // Start from top

    return this.statusReport.map((item: any) => {
      const percentage = item.percentage / 100;
      const angle = percentage * 360;
      const startAngle = currentAngle;
      const endAngle = currentAngle + angle;

      // Convert angles to radians
      const startAngleRad = (startAngle * Math.PI) / 180;
      const endAngleRad = (endAngle * Math.PI) / 180;

      // Calculate start and end points
      const x1 = centerX + radius * Math.cos(startAngleRad);
      const y1 = centerY + radius * Math.sin(startAngleRad);
      const x2 = centerX + radius * Math.cos(endAngleRad);
      const y2 = centerY + radius * Math.sin(endAngleRad);

      // Large arc flag (1 if angle > 180, 0 otherwise)
      const largeArcFlag = angle > 180 ? 1 : 0;

      // Create path
      const path = `M ${centerX} ${centerY} L ${x1} ${y1} A ${radius} ${radius} 0 ${largeArcFlag} 1 ${x2} ${y2} Z`;

      currentAngle += angle;

      return {
        status: item.status,
        count: item.count,
        percentage: item.percentage,
        label: this.getStatusLabel(item.status),
        color: this.getStatusColor(item.status),
        path: path
      };
    });
  }
}
