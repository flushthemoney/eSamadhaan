import { Component, OnInit } from '@angular/core';
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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { CategoryService } from '../../../../services/category.service';
import { AuthService } from '../../../../core/services/auth.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { ConfirmationDialogComponent } from '../../../../shared/components/confirmation-dialog/confirmation-dialog';
import { CategoryDto, CreateCategoryRequestDto, UpdateCategoryRequestDto } from '../../../../models/category';

@Component({
  selector: 'app-category-management',
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
  templateUrl: './category-management.html',
  styleUrl: './category-management.scss',
})
export class CategoryManagementComponent implements OnInit {
  displayedColumns: string[] = ['name', 'description', 'actions'];
  categories: CategoryDto[] = [];
  filteredCategories: CategoryDto[] = [];
  isLoading = false;
  isSubmitting = false;
  showForm = false;
  editingCategory: CategoryDto | null = null;
  categoryForm: FormGroup;
  pageSize = 25;
  pageIndex = 0;
  departmentId: number | null = null;

  constructor(
    private categoryService: CategoryService,
    private authService: AuthService,
    private notificationService: NotificationService,
    private dialog: MatDialog,
    private fb: FormBuilder
  ) {
    this.departmentId = this.authService.departmentId;
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    });
  }

  ngOnInit(): void {
    if (this.departmentId) {
      this.loadCategories();
    }
  }

  loadCategories(): void {
    if (!this.departmentId) return;
    this.isLoading = true;
    this.categoryService.getCategoriesByDepartment(this.departmentId).subscribe({
      next: (data) => {
        this.categories = data;
        this.applyPagination();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  showCreateForm(): void {
    this.editingCategory = null;
    this.categoryForm.reset();
    this.showForm = true;
  }

  showEditForm(category: CategoryDto): void {
    this.editingCategory = category;
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description,
    });
    this.showForm = true;
  }

  cancelForm(): void {
    this.showForm = false;
    this.editingCategory = null;
    this.categoryForm.reset();
  }

  onSubmit(): void {
    if (this.categoryForm.invalid || !this.departmentId) return;

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
        departmentId: this.departmentId,
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

  canEdit(category: CategoryDto): boolean {
    return category.departmentId === this.departmentId;
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = this.pageIndex * this.pageSize;
    const end = start + this.pageSize;
    this.filteredCategories = this.categories.slice(start, end);
  }
}
