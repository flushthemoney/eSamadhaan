import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDividerModule } from '@angular/material/divider';
import { FormsModule } from '@angular/forms';

import { GrievanceService } from '../../../../services/grievance.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { GrievanceResponseDto, GrievanceStatusHistoryDto, ResolutionDto } from '../../../../models/grievance';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';
import { GrievanceStatus, GrievanceStatusLabels } from '../../../../models/common';

@Component({
  selector: 'app-grievance-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatDividerModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './grievance-detail.html',
  styleUrl: './grievance-detail.scss',
})
export class GrievanceDetailComponent implements OnInit {
  grievance: GrievanceResponseDto | null = null;
  history: GrievanceStatusHistoryDto[] = [];
  resolution: ResolutionDto | null = null;
  canEscalate = false;
  isLoading = false;
  isEscalating = false;
  escalationReason = '';
  showEscalationForm = false;
  GrievanceStatus = GrievanceStatus;
  GrievanceStatusLabels = GrievanceStatusLabels;

  constructor(
    private route: ActivatedRoute,
    private grievanceService: GrievanceService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadGrievanceDetail(+id);
      this.loadGrievanceHistory(+id);
      this.loadGrievanceResolution(+id);
      this.checkCanEscalate(+id);
    }
  }

  loadGrievanceDetail(id: number): void {
    this.isLoading = true;
    this.cdr.markForCheck();
    this.grievanceService.getGrievanceById(id).subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.grievance = data;
          this.isLoading = false;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        }, 0);
        this.notificationService.showError('Failed to load grievance details');
      },
    });
  }

  loadGrievanceHistory(id: number): void {
    this.grievanceService.getGrievanceHistory(id).subscribe({
      next: (data) => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.history = data;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        // History load error is not critical
      },
    });
  }

  loadGrievanceResolution(id: number): void {
    this.grievanceService.getGrievanceResolution(id).subscribe({
      next: (data) => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.resolution = data;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        // Resolution may not exist yet
      },
    });
  }

  checkCanEscalate(id: number): void {
    this.grievanceService.canEscalate(id).subscribe({
      next: (data) => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.canEscalate = data.canEscalate;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.canEscalate = false;
          this.cdr.detectChanges();
        }, 0);
      },
    });
  }

  showEscalateForm(): void {
    this.showEscalationForm = true;
  }

  escalateGrievance(): void {
    if (!this.escalationReason.trim()) {
      this.notificationService.showError('Please provide a reason for escalation');
      return;
    }

    if (!this.grievance) return;

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Escalate Grievance',
        message: 'Are you sure you want to escalate this grievance?',
        confirmText: 'Escalate',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed && this.grievance) {
        this.isEscalating = true;
        this.grievanceService.escalateGrievance(this.grievance.id, this.escalationReason).subscribe({
          next: () => {
            this.notificationService.showSuccess('Grievance escalated successfully');
            this.showEscalationForm = false;
            this.escalationReason = '';
            if (this.grievance) {
              this.loadGrievanceDetail(this.grievance.id);
              this.checkCanEscalate(this.grievance.id);
            }
            this.isEscalating = false;
          },
          error: () => {
            this.isEscalating = false;
            this.notificationService.showError('Failed to escalate grievance');
          },
        });
      }
    });
  }

  getStatusLabel(status: GrievanceStatus): string {
    return GrievanceStatusLabels[status] || 'Unknown';
  }
}
