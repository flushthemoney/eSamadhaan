import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';

import { UserService } from '../../../../services/user.service';
import { DepartmentService } from '../../../../services/department.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { UserDto, CreateUserRequest, UpdateUserRequest, UpdateUserStatusRequest } from '../../../../models/user';
import { DepartmentDto } from '../../../../models/department';
import { CustomValidators } from '../../../../core/validators/custom.validators';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatCheckboxModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
  ],
  templateUrl: './user-management.html',
  styleUrl: './user-management.scss',
})
export class UserManagementComponent implements OnInit {
  displayedColumns: string[] = ['name', 'email', 'role', 'departmentName', 'isActive', 'actions'];
  users: UserDto[] = [];
  filteredUsers: UserDto[] = [];
  departments: DepartmentDto[] = [];
  isLoading = false;
  isSubmitting = false;
  showForm = false;
  editingUser: UserDto | null = null;
  userForm: FormGroup;
  pageSize = 25;
  pageIndex = 0;
  roles = [
    { value: 'DepartmentOfficer', label: 'Department Officer' },
    { value: 'SupervisoryOfficer', label: 'Supervisory Officer' },
    { value: 'SystemAdmin', label: 'System Admin' },
  ];

  constructor(
    private userService: UserService,
    private departmentService: DepartmentService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.userForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: [''],
      role: ['', Validators.required],
      departmentId: [''],
      isActive: [true],
    });

    this.userForm.get('role')?.valueChanges.subscribe((role) => {
      const deptControl = this.userForm.get('departmentId');
      if (role === 'DepartmentOfficer') {
        deptControl?.setValidators([Validators.required]);
        deptControl?.enable();
      } else {
        deptControl?.clearValidators();
        deptControl?.setValue(null);
        deptControl?.disable();
      }
      deptControl?.updateValueAndValidity();
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.userService.getAllUsers().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.users = data;
          this.applyPagination();
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

  loadDepartments(): void {
    this.departmentService.getAllDepartments().subscribe({
      next: (data) => {
        this.departments = data;
      },
      error: () => {},
    });
  }

  showCreateForm(): void {
    this.editingUser = null;
    this.userForm.reset({ isActive: true });
    this.userForm.get('password')?.setValidators([Validators.required, CustomValidators.passwordStrength]);
    this.userForm.get('password')?.updateValueAndValidity();
    this.showForm = true;
  }

  showEditForm(user: UserDto): void {
    this.editingUser = user;
    this.userForm.patchValue({
      name: user.name,
      email: user.email,
      role: user.role,
      departmentId: user.departmentId,
      isActive: user.isActive,
    });
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingUser = null;
    this.userForm.reset({ isActive: true });
    this.userForm.get('password')?.setValidators([Validators.required, CustomValidators.passwordStrength]);
    this.userForm.get('password')?.updateValueAndValidity();
  }

  onSubmit(): void {
    if (this.userForm.invalid) return;

    this.isSubmitting = true;
    if (this.editingUser) {
      const request: UpdateUserRequest = {
        name: this.userForm.value.name,
        email: this.userForm.value.email,
        role: this.userForm.value.role,
        departmentId: this.userForm.value.departmentId || null,
      };
      this.userService.updateUser(this.editingUser.id, request).subscribe({
        next: () => {
          this.notificationService.showSuccess('User updated successfully');
          this.cancelForm();
          this.loadUsers();
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
          this.notificationService.showError('Failed to update user');
        },
      });
    } else {
      const request: CreateUserRequest = {
        name: this.userForm.value.name,
        email: this.userForm.value.email,
        password: this.userForm.value.password,
        role: this.userForm.value.role,
        departmentId: this.userForm.value.departmentId || null,
      };
      this.userService.createUser(request).subscribe({
        next: () => {
          this.notificationService.showSuccess('User created successfully');
          this.cancelForm();
          this.loadUsers();
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
          this.notificationService.showError('Failed to create user');
        },
      });
    }
  }

  toggleUserStatus(user: UserDto): void {
    const request: UpdateUserStatusRequest = {
      isActive: !user.isActive,
    };
    this.userService.updateUserStatus(user.id, request).subscribe({
      next: () => {
        this.notificationService.showSuccess(`User ${user.isActive ? 'deactivated' : 'activated'} successfully`);
        this.loadUsers();
      },
      error: () => {
        this.notificationService.showError('Failed to update user status');
      },
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    this.filteredUsers = this.users.slice(start, end);
  }
}
