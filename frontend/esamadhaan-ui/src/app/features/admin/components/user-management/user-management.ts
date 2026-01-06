import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
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
import { AuthService } from '../../../../core/services/auth.service';
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
    FormsModule,
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
export class UserManagementComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['name', 'email', 'role', 'departmentName', 'isActive', 'actions'];
  users: UserDto[] = [];
  allUsers: UserDto[] = [];
  dataSource = new MatTableDataSource<UserDto>([]);
  departments: DepartmentDto[] = [];
  selectedRole: string | null = null;
  selectedDepartmentId: number | null = null;
  isLoading = false;
  isSubmitting = false;
  showForm = false;
  editingUser: UserDto | null = null;
  userForm: FormGroup;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  roles = [
    { value: 'DepartmentOfficer', label: 'Department Officer' },
    { value: 'SupervisoryOfficer', label: 'Supervisory Officer' },
    { value: 'SystemAdmin', label: 'System Admin' },
  ];

  constructor(
    private userService: UserService,
    private departmentService: DepartmentService,
    private notificationService: NotificationService,
    private authService: AuthService,
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
          this.allUsers = data;
          this.users = data;
          this.dataSource.data = this.users;
          this.isLoading = false;
          this.cdr.detectChanges();
          // Connect paginator after view updates
          this.connectPaginatorWithRetry();
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

  onFilterChange(): void {
    // Reset department filter if role is not DepartmentOfficer
    if (this.selectedRole !== 'DepartmentOfficer') {
      this.selectedDepartmentId = null;
    }

    let filtered = [...this.allUsers];

    // Filter by role
    if (this.selectedRole) {
      filtered = filtered.filter(user => user.role === this.selectedRole);
    }

    // Filter by department if role is DepartmentOfficer and department is selected
    if (this.selectedRole === 'DepartmentOfficer' && this.selectedDepartmentId) {
      filtered = filtered.filter(user => user.departmentId === this.selectedDepartmentId);
    }

    this.users = filtered;
    this.dataSource.data = this.users;
    
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.cdr.detectChanges();
  }
  
  ngAfterViewInit(): void {
    // Try to connect paginator when view is initialized
    this.connectPaginatorWithRetry();
  }

  private connectPaginatorWithRetry(attempts = 0): void {
    const maxAttempts = 10;
    if (attempts >= maxAttempts) {
      return;
    }

    setTimeout(() => {
      if (this.paginator && this.dataSource) {
        this.dataSource.paginator = this.paginator;
        this.cdr.detectChanges();
      } else if (attempts < maxAttempts) {
        // Retry if paginator not available yet (might be conditionally rendered)
        this.connectPaginatorWithRetry(attempts + 1);
      }
    }, 50);
  }

  loadDepartments(): void {
    this.departmentService.getAllDepartments().subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.departments = data;
          this.cdr.markForCheck();
        }, 0);
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

  isCurrentUser(user: UserDto): boolean {
    const currentUserId = this.authService.userId;
    return currentUserId !== null && user.id === currentUserId;
  }

  toggleUserStatus(user: UserDto): void {
    if (this.isCurrentUser(user)) {
      this.notificationService.showError('You cannot change your own status');
      return;
    }

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

}
