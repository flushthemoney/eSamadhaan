import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { DepartmentService } from '../../../../services/department.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { DepartmentDto, CreateDepartmentRequestDto, UpdateDepartmentRequestDto } from '../../../../models/department';

@Component({
  selector: 'app-department-management',
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
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
  ],
  templateUrl: './department-management.html',
  styleUrl: './department-management.scss',
})
export class DepartmentManagementComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['name', 'description', 'actions'];
  departments: DepartmentDto[] = [];
  dataSource = new MatTableDataSource<DepartmentDto>([]);
  isLoading = false;
  isSubmitting = false;
  showForm = false;
  editingDepartment: DepartmentDto | null = null;
  departmentForm: FormGroup;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private departmentService: DepartmentService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.departmentForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
  }

  loadDepartments(): void {
    this.isLoading = true;
    this.departmentService.getAllDepartments().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.departments = data;
          this.dataSource.data = this.departments;
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

  showCreateForm(): void {
    this.editingDepartment = null;
    this.departmentForm.reset();
    this.showForm = true;
  }

  showEditForm(department: DepartmentDto): void {
    this.editingDepartment = department;
    this.departmentForm.patchValue({
      name: department.name,
      description: department.description,
    });
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingDepartment = null;
    this.departmentForm.reset();
  }

  onSubmit(): void {
    if (this.departmentForm.invalid) return;

    this.isSubmitting = true;
    if (this.editingDepartment) {
      const request: UpdateDepartmentRequestDto = {
        name: this.departmentForm.value.name,
        description: this.departmentForm.value.description,
      };
      this.departmentService.updateDepartment(this.editingDepartment.id, request).subscribe({
        next: () => {
          this.notificationService.showSuccess('Department updated successfully');
          this.cancelForm();
          this.loadDepartments();
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
          this.notificationService.showError('Failed to update department');
        },
      });
    } else {
      const request: CreateDepartmentRequestDto = {
        name: this.departmentForm.value.name,
        description: this.departmentForm.value.description,
      };
      this.departmentService.createDepartment(request).subscribe({
        next: () => {
          this.notificationService.showSuccess('Department created successfully');
          this.cancelForm();
          this.loadDepartments();
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
          this.notificationService.showError('Failed to create department');
        },
      });
    }
  }

  deleteDepartment(department: DepartmentDto): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Department',
        message: `Are you sure you want to delete "${department.name}"? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.departmentService.deleteDepartment(department.id).subscribe({
          next: () => {
            this.notificationService.showSuccess('Department deleted successfully');
            this.loadDepartments();
          },
          error: () => {
            this.notificationService.showError('Failed to delete department');
          },
        });
      }
    });
  }

}
