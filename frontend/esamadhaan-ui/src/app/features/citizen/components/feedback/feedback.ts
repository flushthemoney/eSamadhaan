import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';

import { GrievanceService } from '../../../../services/grievance.service';
import { FeedbackService } from '../../../../services/feedback.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceListDto } from '../../../../models/grievance';
import { GrievanceStatus } from '../../../../models/common';
import { CreateFeedbackRequest } from '../../../../models/feedback';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-feedback',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatDialogModule,
    MatDividerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './feedback.html',
  styleUrl: './feedback.scss',
})
export class FeedbackComponent implements OnInit {
  resolvedGrievances: GrievanceListDto[] = [];
  closedGrievances: GrievanceListDto[] = [];
  eligibleGrievances: GrievanceListDto[] = [];
  feedbackForms: Map<number, FormGroup> = new Map();
  isLoading = false;
  submittingFeedback = new Set<number>();

  constructor(
    private grievanceService: GrievanceService,
    private feedbackService: FeedbackService,
    private notificationService: NotificationService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.loadEligibleGrievances();
  }

  loadEligibleGrievances(): void {
    this.isLoading = true;
    
    // Load resolved grievances
    this.grievanceService.getMyGrievances(GrievanceStatus.Resolved).subscribe({
      next: (data) => {
        this.resolvedGrievances = data;
        this.combineGrievances();
      },
      error: () => {
        this.isLoading = false;
      },
    });

    // Load closed grievances
    this.grievanceService.getMyGrievances(GrievanceStatus.Closed).subscribe({
      next: (data) => {
        this.closedGrievances = data;
        this.combineGrievances();
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  combineGrievances(): void {
    this.eligibleGrievances = [...this.resolvedGrievances, ...this.closedGrievances];
    
    // Check feedback for each grievance
    this.eligibleGrievances.forEach((grievance) => {
      this.checkExistingFeedback(grievance.id);
      this.createFeedbackForm(grievance.id);
    });
    
    this.isLoading = false;
  }

  createFeedbackForm(grievanceId: number): void {
    if (!this.feedbackForms.has(grievanceId)) {
      const form = this.fb.group({
        rating: [0, [Validators.required, Validators.min(1), Validators.max(5)]],
        comments: ['', [Validators.maxLength(500)]],
      });
      this.feedbackForms.set(grievanceId, form);
    }
  }

  checkExistingFeedback(grievanceId: number): void {
    this.feedbackService.getFeedback(grievanceId).subscribe({
      next: () => {
        // Feedback exists, mark grievance as feedback submitted
        const grievance = this.eligibleGrievances.find(g => g.id === grievanceId);
        if (grievance) {
          (grievance as any).feedbackSubmitted = true;
        }
      },
      error: () => {
        // No feedback exists, allow submission
      },
    });
  }

  setRating(grievanceId: number, rating: number): void {
    const form = this.feedbackForms.get(grievanceId);
    if (form) {
      form.patchValue({ rating });
    }
  }

  submitFeedback(grievanceId: number): void {
    const form = this.feedbackForms.get(grievanceId);
    if (!form || form.invalid) {
      form?.markAllAsTouched();
      return;
    }

    this.submittingFeedback.add(grievanceId);
    const request: CreateFeedbackRequest = {
      rating: form.value.rating,
      comment: form.value.comments || null,
    };

    this.feedbackService.submitFeedback(grievanceId, request).subscribe({
      next: () => {
        this.notificationService.showSuccess('Feedback submitted successfully');
        const grievance = this.eligibleGrievances.find(g => g.id === grievanceId);
        if (grievance) {
          (grievance as any).feedbackSubmitted = true;
        }
        this.submittingFeedback.delete(grievanceId);
      },
      error: () => {
        this.notificationService.showError('Failed to submit feedback');
        this.submittingFeedback.delete(grievanceId);
      },
    });
  }

  hasFeedback(grievanceId: number): boolean {
    const grievance = this.eligibleGrievances.find(g => g.id === grievanceId);
    return (grievance as any)?.feedbackSubmitted || false;
  }
}
