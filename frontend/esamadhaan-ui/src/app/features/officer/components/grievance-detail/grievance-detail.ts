import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDividerModule } from '@angular/material/divider';

import { GrievanceService } from '../../../../services/grievance.service';
import { OfficerService } from '../../../../services/officer.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { GrievanceStatus } from '../../../../models/common';
import { CreateAssignmentRequest } from '../../../../models/assignment';
import { CreateResolutionRequest } from '../../../../models/resolution';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-officer-grievance-detail',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
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
  grievance: any = null;
  officers: any[] = [];
  isLoading = false;
  isLoadingOfficers = false;
  GrievanceStatus = GrievanceStatus;

  // Assignment
  showAssignForm = false;
  assignForm: FormGroup;
  isAssigning = false;

  // Resolution
  showResolutionForm = false;
  resolutionForm: FormGroup;
  isResolving = false;

  // Status update
  isUpdatingStatus = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private grievanceService: GrievanceService,
    private officerService: OfficerService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private fb: FormBuilder
  ) {
    this.assignForm = this.fb.group({
      officerId: ['', [Validators.required]],
    });

    this.resolutionForm = this.fb.group({
      remarks: ['', [Validators.required, Validators.minLength(50), Validators.maxLength(1000)]],
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadGrievanceDetail(+id);
      this.loadOfficers();
    }
  }

  loadGrievanceDetail(id: number): void {
    this.isLoading = true;
    this.grievanceService.getOfficerGrievanceDetail(id).subscribe({
      next: (data) => {
        this.grievance = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.notificationService.showError('Failed to load grievance details');
      },
    });
  }

  loadOfficers(): void {
    this.isLoadingOfficers = true;
    this.officerService.getDepartmentOfficers().subscribe({
      next: (data) => {
        this.officers = data.filter((o: any) => o.isActive);
        this.isLoadingOfficers = false;
      },
      error: () => {
        this.isLoadingOfficers = false;
      },
    });
  }

  showAssignDialog(): void {
    this.showAssignForm = true;
  }

  assignGrievance(): void {
    if (this.assignForm.invalid || !this.grievance) return;

    this.isAssigning = true;
    const request: CreateAssignmentRequest = {
      officerId: this.assignForm.value.officerId,
    };

    this.grievanceService.assignGrievance(this.grievance.id, request).subscribe({
      next: () => {
        this.notificationService.showSuccess('Grievance assigned successfully');
        this.showAssignForm = false;
        this.assignForm.reset();
        this.loadGrievanceDetail(this.grievance.id);
        this.isAssigning = false;
      },
      error: () => {
        this.isAssigning = false;
        this.notificationService.showError('Failed to assign grievance');
      },
    });
  }

  updateStatus(newStatus: GrievanceStatus): void {
    if (!this.grievance) return;

    const statusNames: Record<GrievanceStatus, string> = {
      [GrievanceStatus.Submitted]: 'Submitted',
      [GrievanceStatus.Assigned]: 'Assigned',
      [GrievanceStatus.InReview]: 'In Review',
      [GrievanceStatus.Resolved]: 'Resolved',
      [GrievanceStatus.Closed]: 'Closed',
    };

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Update Status',
        message: `Are you sure you want to change status to "${statusNames[newStatus]}"?`,
        confirmText: 'Update',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.isUpdatingStatus = true;
        this.grievanceService.updateGrievanceStatus(this.grievance.id, newStatus).subscribe({
          next: () => {
            this.notificationService.showSuccess('Status updated successfully');
            this.loadGrievanceDetail(this.grievance.id);
            this.isUpdatingStatus = false;
          },
          error: () => {
            this.isUpdatingStatus = false;
            this.notificationService.showError('Failed to update status');
          },
        });
      }
    });
  }

  showResolutionDialog(): void {
    this.showResolutionForm = true;
  }

  submitResolution(): void {
    if (this.resolutionForm.invalid || !this.grievance) return;

    this.isResolving = true;
    const request: CreateResolutionRequest = {
      resolutionRemarks: this.resolutionForm.value.remarks,
    };

    this.grievanceService.submitResolution(this.grievance.id, request).subscribe({
      next: () => {
        this.notificationService.showSuccess('Resolution submitted successfully');
        this.showResolutionForm = false;
        this.resolutionForm.reset();
        this.loadGrievanceDetail(this.grievance.id);
        this.isResolving = false;
      },
      error: () => {
        this.isResolving = false;
        this.notificationService.showError('Failed to submit resolution');
      },
    });
  }

  closeGrievance(): void {
    if (!this.grievance) return;

    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Close Grievance',
        message: 'Are you sure you want to close this grievance? This action cannot be undone.',
        confirmText: 'Close',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.grievanceService.closeGrievance(this.grievance.id).subscribe({
          next: () => {
            this.notificationService.showSuccess('Grievance closed successfully');
            this.loadGrievanceDetail(this.grievance.id);
          },
          error: () => {
            this.notificationService.showError('Failed to close grievance');
          },
        });
      }
    });
  }

  get isUnassigned(): boolean {
    return !this.grievance?.currentAssignment;
  }
}
