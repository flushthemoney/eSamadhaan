import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { CustomValidators } from '../../../../core/validators/custom.validators';
import { ChangePasswordRequest } from '../../../../models/auth';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './change-password.html',
  styleUrl: './change-password.scss',
})
export class ChangePasswordComponent implements OnInit {
  changePasswordForm: FormGroup;
  isSubmitting = false;
  submitAttempted = false;
  hideCurrentPassword = true;
  hideNewPassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.changePasswordForm = this.fb.group(
      {
        currentPassword: ['', [Validators.required, Validators.minLength(8)]],
        newPassword: [
          '',
          [
            Validators.required,
            Validators.minLength(8),
            Validators.maxLength(50),
            CustomValidators.passwordStrength,
          ],
        ],
        confirmPassword: ['', [Validators.required]],
      },
      {
        validators: [
          CustomValidators.passwordMatch,
          CustomValidators.newPasswordDifferent,
        ],
      }
    );
  }

  ngOnInit(): void {
    if (!this.authService.isAuthenticated) {
      this.router.navigate(['/auth/login']);
    }
  }

  onSubmit(): void {
    this.submitAttempted = true;

    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const changePasswordData: ChangePasswordRequest = {
      currentPassword: this.changePasswordForm.value.currentPassword,
      newPassword: this.changePasswordForm.value.newPassword,
    };

    this.authService.changePassword(changePasswordData).subscribe({
      next: () => {
        this.notificationService.showSuccess('Password changed successfully');
        this.changePasswordForm.reset();
        this.submitAttempted = false;
        this.isSubmitting = false;
      },
      error: (error) => {
        this.isSubmitting = false;
        if (error.status === 400) {
          const errorMessage =
            error.error?.message || 'Failed to change password. Please check your current password.';
          this.notificationService.showError(errorMessage);
        } else if (error.status === 0) {
          this.notificationService.showError(
            'Unable to connect. Please check your internet connection.'
          );
        } else {
          this.notificationService.showError('An error occurred while changing password');
        }
      },
    });
  }

  shouldShowError(fieldName: string): boolean {
    const field = this.changePasswordForm.get(fieldName);
    return !!(field && (field.touched || this.submitAttempted) && field.invalid);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.changePasswordForm.get(fieldName);
    if (!field || !field.errors) {
      // Check for form-level errors
      if (
        fieldName === 'confirmPassword' &&
        this.changePasswordForm.errors?.['passwordMismatch']
      ) {
        return 'Passwords do not match';
      }
      if (
        fieldName === 'newPassword' &&
        this.changePasswordForm.errors?.['samePassword']
      ) {
        return 'New password must be different from current password';
      }
      return '';
    }

    const errors = field.errors;
    if (errors['required']) {
      if (fieldName === 'currentPassword') return 'Current password is required';
      if (fieldName === 'newPassword') return 'New password is required';
      if (fieldName === 'confirmPassword') return 'Please confirm your password';
    }
    if (errors['minlength']) {
      return 'Password must be at least 8 characters';
    }
    if (errors['passwordStrength']) {
      return 'Password must contain uppercase, lowercase, number, and special character';
    }

    return 'Invalid value';
  }
}
