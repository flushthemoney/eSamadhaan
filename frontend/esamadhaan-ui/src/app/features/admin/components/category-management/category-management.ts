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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CategoryService } from '../../../../services/category.service';
import { DepartmentService } from '../../../../services/department.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { CategoryDto, CreateCategoryRequestDto, UpdateCategoryRequestDto } from '../../../../models/category';
import { DepartmentDto } from '../../../../models/department';

@Component({
  selector: 'app-category-management',
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
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
  ],
  templateUrl: './category-management.html',
  styleUrl: './category-management.scss',
})
export class CategoryManagementComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['name', 'description', 'departmentName', 'actions'];
  categories: CategoryDto[] = [];
  allCategories: CategoryDto[] = [];
  dataSource = new MatTableDataSource<CategoryDto>([]);
  departments: DepartmentDto[] = [];
  selectedDepartmentId: number | null = null;
  isLoading = false;
  isSubmitting = false;
  showForm = false;
  editingCategory: CategoryDto | null = null;
  categoryForm: FormGroup;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private categoryService: CategoryService,
    private departmentService: DepartmentService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
      departmentId: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.categoryService.getAllCategories().subscribe({
      next: (data: CategoryDto[]) => {
        setTimeout(() => {
          this.allCategories = data;
          this.categories = data;
          this.dataSource.data = this.categories;
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
    if (this.selectedDepartmentId) {
      this.categories = this.allCategories.filter(cat => cat.departmentId === this.selectedDepartmentId);
    } else {
      this.categories = [...this.allCategories];
    }
    this.dataSource.data = this.categories;
    
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
    this.editingCategory = null;
    this.categoryForm.reset();
    this.categoryForm.get('departmentId')?.enable();
    this.showForm = true;
  }

  showEditForm(category: CategoryDto): void {
    this.editingCategory = category;
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description,
      departmentId: category.departmentId,
    });
    this.categoryForm.get('departmentId')?.disable();
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingCategory = null;
    this.categoryForm.reset();
    this.categoryForm.get('departmentId')?.enable();
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) return;

    this.isSubmitting = true;
    if (this.editingCategory) {
      const request: UpdateCategoryRequestDto = {
        name: this.categoryForm.value.name,
        description: this.categoryForm.value.description,
      };
      this.categoryService.updateCategory(this.editingCategory.id, request).subscribe({
        next: () => {
          this.notificationService.showSuccess('Category updated successfully');
          this.cancelForm();
          this.loadCategories();
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
          this.notificationService.showError('Failed to update category');
        },
      });
    } else {
      const request: CreateCategoryRequestDto = {
        name: this.categoryForm.value.name,
        description: this.categoryForm.value.description,
        departmentId: this.categoryForm.value.departmentId,
      };
      this.categoryService.createCategory(request).subscribe({
        next: () => {
          this.notificationService.showSuccess('Category created successfully');
          this.cancelForm();
          this.loadCategories();
          this.isSubmitting = false;
        },
        error: () => {
          this.isSubmitting = false;
          this.notificationService.showError('Failed to create category');
        },
      });
    }
  }

  deleteCategory(category: CategoryDto): void {
    const dialogRef = this.dialog.open(ConfirmationDialogComponent, {
      width: '400px',
      data: {
        title: 'Delete Category',
        message: `Are you sure you want to delete "${category.name}"? This action cannot be undone.`,
        confirmText: 'Delete',
        cancelText: 'Cancel',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed) => {
      if (confirmed) {
        this.categoryService.deleteCategory(category.id).subscribe({
          next: () => {
            this.notificationService.showSuccess('Category deleted successfully');
            this.loadCategories();
          },
          error: () => {
            this.notificationService.showError('Failed to delete category');
          },
        });
      }
    });
  }

}
