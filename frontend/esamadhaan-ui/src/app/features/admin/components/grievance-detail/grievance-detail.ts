import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';

import { GrievanceService } from '../../../../services/grievance.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceStatus, GrievanceStatusLabels } from '../../../../models/common';
import { OfficerStatusHistoryDto } from '../../../../models/grievance';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-admin-grievance-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './grievance-detail.html',
  styleUrl: './grievance-detail.scss',
})
export class AdminGrievanceDetailComponent implements OnInit {
  grievance: any = null;
  isLoading = false;
  GrievanceStatus = GrievanceStatus;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private grievanceService: GrievanceService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadGrievanceDetail(+id);
    }
  }

  loadGrievanceDetail(id: number): void {
    this.isLoading = true;
    this.grievanceService.getGrievanceDetailForSupervisor(id).subscribe({
      next: (data) => {
        this.grievance = data;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.markForCheck();
      },
    });
  }

  getStatusLabel(status: GrievanceStatus): string {
    return GrievanceStatusLabels[status] || 'Unknown';
  }

  getStatusHistoryLabel(item: OfficerStatusHistoryDto): string {
    // Check if this is a reassignment by looking at the remarks
    if (
      item.newStatus === GrievanceStatus.Assigned &&
      item.remarks &&
      item.remarks.toLowerCase().includes('reassigned')
    ) {
      return 'Reassigned';
    }
    return this.getStatusLabel(item.newStatus);
  }

  goBack(): void {
    this.router.navigate(['/admin/grievances']);
  }
}

