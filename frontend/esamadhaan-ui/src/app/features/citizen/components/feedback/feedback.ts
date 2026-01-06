import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
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
import { CreateFeedbackRequest, FeedbackResponseDto } from '../../../../models/feedback';
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
  feedbackData: Map<number, FeedbackResponseDto> = new Map();
  isLoading = false;
  submittingFeedback = new Set<number>();

  constructor(
    private grievanceService: GrievanceService,
    private feedbackService: FeedbackService,
    private notificationService: NotificationService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadEligibleGrievances();
  }

  loadEligibleGrievances(): void {
    this.isLoading = true;
    this.cdr.markForCheck();
    
    let resolvedLoaded = false;
    let closedLoaded = false;
    
    const checkAndCombine = () => {
      if (resolvedLoaded && closedLoaded) {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.combineGrievances();
          this.cdr.detectChanges();
        }, 0);
      }
    };
    
    // Load resolved grievances
    this.grievanceService.getMyGrievances(GrievanceStatus.Resolved).subscribe({
      next: (data) => {
        this.resolvedGrievances = data;
        resolvedLoaded = true;
        checkAndCombine();
      },
      error: () => {
        resolvedLoaded = true;
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        }, 0);
      },
    });

    // Load closed grievances
    this.grievanceService.getMyGrievances(GrievanceStatus.Closed).subscribe({
      next: (data) => {
        this.closedGrievances = data;
        closedLoaded = true;
        checkAndCombine();
      },
      error: () => {
        closedLoaded = true;
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.detectChanges();
        }, 0);
      },
    });
  }

  combineGrievances(): void {
    this.eligibleGrievances = [...this.resolvedGrievances, ...this.closedGrievances];
    
    // Sort by newest first (reverse chronological order)
    this.eligibleGrievances.sort((a, b) => {
      const dateA = new Date(a.createdAt).getTime();
      const dateB = new Date(b.createdAt).getTime();
      return dateB - dateA; // Newest first
    });
    
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
      next: (feedback: FeedbackResponseDto) => {
        // Feedback exists, store feedback data and mark grievance as feedback submitted
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.feedbackData.set(grievanceId, feedback);
          const grievance = this.eligibleGrievances.find(g => g.id === grievanceId);
          if (grievance) {
            (grievance as any).feedbackSubmitted = true;
            this.cdr.markForCheck();
          }
        }, 0);
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
      next: (feedback: FeedbackResponseDto) => {
        this.notificationService.showSuccess('Feedback submitted successfully');
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.feedbackData.set(grievanceId, feedback);
          const grievance = this.eligibleGrievances.find(g => g.id === grievanceId);
          if (grievance) {
            (grievance as any).feedbackSubmitted = true;
          }
          this.submittingFeedback.delete(grievanceId);
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        this.notificationService.showError('Failed to submit feedback');
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.submittingFeedback.delete(grievanceId);
          this.cdr.detectChanges();
        }, 0);
      },
    });
  }

  hasFeedback(grievanceId: number): boolean {
    const grievance = this.eligibleGrievances.find(g => g.id === grievanceId);
    return (grievance as any)?.feedbackSubmitted || false;
  }

  getFeedback(grievanceId: number): FeedbackResponseDto | undefined {
    return this.feedbackData.get(grievanceId);
  }
}
