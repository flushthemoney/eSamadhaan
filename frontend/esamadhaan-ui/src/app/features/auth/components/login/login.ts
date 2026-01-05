import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoginRequest, LoginResponse } from '../../../../models/auth';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCheckboxModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class LoginComponent implements OnInit {
  loginForm: FormGroup;
  isSubmitting = false;
  submitAttempted = false;
  hidePassword = true;
  returnUrl: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private notificationService: NotificationService,
    private router: Router,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
    });
  }

  ngOnInit(): void {
    // Get return URL from route parameters or default to null
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || null;

    // If already authenticated, redirect
    if (this.authService.isAuthenticated) {
      this.authService.navigateByRole();
    }
  }

  onSubmit(): void {
    this.submitAttempted = true;

    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const credentials: LoginRequest = {
      email: this.loginForm.value.email,
      password: this.loginForm.value.password,
    };

    this.authService.login(credentials).subscribe({
      next: (response: LoginResponse) => {
        this.notificationService.showSuccess('Login successful');
        
        // Defer state changes and navigation to avoid change detection errors
        setTimeout(() => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
          
          // Navigate after change detection using role from response
          setTimeout(() => {
            if (this.returnUrl) {
              this.router.navigateByUrl(this.returnUrl);
            } else {
              // Use role from response for immediate navigation
              this.navigateByRole(response.role);
            }
          }, 0);
        }, 0);
      },
      error: (error) => {
        // Defer state change to avoid change detection errors
        setTimeout(() => {
          this.isSubmitting = false;
          this.cdr.markForCheck();
        }, 0);
        
        if (error.status === 401) {
          this.notificationService.showError('Invalid email or password');
        } else if (error.status === 0) {
          this.notificationService.showError(
            'Unable to connect. Please check your internet connection.'
          );
        } else {
          const errorMessage = error.error?.message || 'An error occurred during login';
          this.notificationService.showError(errorMessage);
        }
      },
    });
  }

  get isButtonDisabled(): boolean {
    return this.loginForm.invalid || this.isSubmitting;
  }

  private navigateByRole(role: string): void {
    const routes: Record<string, string> = {
      Citizen: "/citizen/dashboard",
      DepartmentOfficer: "/officer/dashboard",
      SupervisoryOfficer: "/supervisor/dashboard",
      SystemAdmin: "/admin/dashboard",
    };

    const route = routes[role] || "/";
    console.log(`Navigating to ${route} for role: ${role}`);
    this.router.navigate([route]);
  }

  shouldShowError(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    return !!(field && (field.touched || this.submitAttempted) && field.invalid);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (!field || !field.errors) return '';

    const errors = field.errors;
    if (errors['required']) {
      return fieldName === 'email' ? 'Email is required' : 'Password is required';
    }
    if (errors['email']) {
      return 'Please enter a valid email address';
    }
    if (errors['minlength']) {
      return 'Password must be at least 8 characters';
    }

    return 'Invalid value';
  }
}
