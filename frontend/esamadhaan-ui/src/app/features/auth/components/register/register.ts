import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
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
import { RegisterRequest } from '../../../../models/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss',
})
export class RegisterComponent implements OnInit {
  registerForm: FormGroup;
  isSubmitting = false;
  submitAttempted = false;
  hidePassword = true;
  hideConfirmPassword = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.registerForm = this.fb.group(
      {
        name: [
          '',
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(100),
            Validators.pattern(/^[a-zA-Z\s]+$/),
          ],
        ],
        email: [
          '',
          [Validators.required, Validators.email],
          [CustomValidators.emailAsync(this.authService)],
        ],
        password: [
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
        validators: [CustomValidators.passwordMatch],
      }
    );
  }

  ngOnInit(): void {
    // If already authenticated, redirect
    if (this.authService.isAuthenticated) {
      this.authService.navigateByRole();
    }
  }

  onSubmit(): void {
    this.submitAttempted = true;

    if (this.registerForm.invalid || this.registerForm.pending) {
      this.registerForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const registrationData: RegisterRequest = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      password: this.registerForm.value.password,
      role: 'Citizen', // Only citizens can self-register
      departmentId: null,
    };

    this.authService.register(registrationData).subscribe({
      next: () => {
        this.notificationService.showSuccess(
          'Registration successful! Please login.'
        );
        this.router.navigate(['/auth/login']);
        this.isSubmitting = false;
      },
      error: (error) => {
        this.isSubmitting = false;
        if (error.status === 400) {
          const errorMessage =
            error.error?.message || 'Registration failed. Please check your information.';
          this.notificationService.showError(errorMessage);
        } else if (error.status === 0) {
          this.notificationService.showError(
            'Unable to connect. Please check your internet connection.'
          );
        } else {
          this.notificationService.showError('An error occurred during registration');
        }
      },
    });
  }

  shouldShowError(fieldName: string): boolean {
    const field = this.registerForm.get(fieldName);
    return !!(field && (field.touched || this.submitAttempted) && field.invalid);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.registerForm.get(fieldName);
    if (!field || !field.errors) {
      // Check for form-level errors
      if (fieldName === 'confirmPassword' && this.registerForm.errors?.['passwordMismatch']) {
        return 'Passwords do not match';
      }
      return '';
    }

    const errors = field.errors;
    if (errors['required']) {
      if (fieldName === 'name') return 'Full name is required';
      if (fieldName === 'email') return 'Email is required';
      if (fieldName === 'password') return 'Password is required';
      if (fieldName === 'confirmPassword') return 'Please confirm your password';
    }
    if (errors['email']) {
      return 'Please enter a valid email address';
    }
    if (errors['emailTaken']) {
      return 'This email is already registered';
    }
    if (errors['minlength']) {
      if (fieldName === 'name') return 'Name must be at least 3 characters';
      if (fieldName === 'password') return 'Password must be at least 8 characters';
    }
    if (errors['maxlength']) {
      if (fieldName === 'name') return 'Name cannot exceed 100 characters';
      if (fieldName === 'password') return 'Password cannot exceed 50 characters';
    }
    if (errors['pattern']) {
      if (fieldName === 'name') return 'Name can only contain letters and spaces';
    }
    if (errors['passwordStrength']) {
      return 'Password must contain uppercase, lowercase, number, and special character';
    }

    return 'Invalid value';
  }

  getPasswordStrength(password: string): string {
    if (!password) return '';

    let strength = 0;
    if (password.length >= 8) strength++;
    if (/[A-Z]/.test(password)) strength++;
    if (/[a-z]/.test(password)) strength++;
    if (/[0-9]/.test(password)) strength++;
    if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

    const levels = ['', 'Weak', 'Fair', 'Good', 'Strong', 'Very Strong'];
    return levels[strength];
  }

  getPasswordStrengthClass(password: string): string {
    const strength = this.getPasswordStrength(password);
    if (strength === 'Weak' || strength === '') return 'weak';
    if (strength === 'Fair') return 'fair';
    if (strength === 'Good') return 'good';
    if (strength === 'Strong') return 'strong';
    if (strength === 'Very Strong') return 'very-strong';
    return '';
  }
}
