import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';

import { DepartmentService } from '../../../../services/department.service';
import { CategoryService } from '../../../../services/category.service';
import { GrievanceService } from '../../../../services/grievance.service';
import { NotificationService } from '../../../../core/services/notification.service';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { fileValidator } from '../../../../core/validators/file.validator';
import { DepartmentDto } from '../../../../models/department';
import { CategoryDto } from '../../../../models/category';
import { CreateGrievanceRequest } from '../../../../models/grievance';

@Component({
  selector: 'app-lodge-grievance',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatChipsModule,
    MatDividerModule,
    PageHeaderComponent,
  ],
  templateUrl: './lodge-grievance.html',
  styleUrl: './lodge-grievance.scss',
})
export class LodgeGrievanceComponent implements OnInit {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  lodgeGrievanceForm: FormGroup;
  departments: DepartmentDto[] = [];
  categories: CategoryDto[] = [];
  isLoadingDepartments = false;
  isLoadingCategories = false;
  isSubmitting = false;
  submitAttempted = false;
  selectedFile: File | null = null;
  noCategoriesMessage = '';

  constructor(
    private fb: FormBuilder,
    private departmentService: DepartmentService,
    private categoryService: CategoryService,
    private grievanceService: GrievanceService,
    private notificationService: NotificationService,
    private router: Router
  ) {
    this.lodgeGrievanceForm = this.fb.group({
      departmentId: ['', [Validators.required]],
      categoryId: ['', [Validators.required]],
      description: [
        '',
        [
          Validators.required,
          Validators.minLength(50),
          Validators.maxLength(2000),
        ],
      ],
      attachment: [null, [fileValidator]],
    });
  }

  ngOnInit(): void {
    this.loadDepartments();

    this.lodgeGrievanceForm.get('departmentId')?.valueChanges.subscribe((departmentId) => {
      this.lodgeGrievanceForm.patchValue({ categoryId: '' });
      this.lodgeGrievanceForm.get('categoryId')?.disable();
      this.categories = [];
      this.noCategoriesMessage = '';

      if (departmentId) {
        this.loadCategories(departmentId);
      }
    });
  }

  loadDepartments(): void {
    this.isLoadingDepartments = true;
    this.departmentService.getAllDepartments().subscribe({
      next: (departments) => {
        this.departments = departments;
        this.isLoadingDepartments = false;
      },
      error: () => {
        this.isLoadingDepartments = false;
        this.notificationService.showError('Failed to load departments');
      },
    });
  }

  loadCategories(departmentId: number): void {
    this.isLoadingCategories = true;
    this.categoryService.getCategoriesByDepartment(departmentId).subscribe({
      next: (categories) => {
        this.categories = categories;
        this.lodgeGrievanceForm.get('categoryId')?.enable();
        this.isLoadingCategories = false;

        if (categories.length === 0) {
          this.noCategoriesMessage = 'No categories available for this department';
        }
      },
      error: () => {
        this.isLoadingCategories = false;
        this.notificationService.showError('Failed to load categories');
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      this.selectedFile = file;
      this.lodgeGrievanceForm.patchValue({ attachment: file });
      this.lodgeGrievanceForm.get('attachment')?.updateValueAndValidity();
    }
  }

  removeFile(): void {
    this.selectedFile = null;
    this.lodgeGrievanceForm.patchValue({ attachment: null });
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  onSubmit(): void {
    this.submitAttempted = true;

    if (
      this.lodgeGrievanceForm.invalid ||
      this.categories.length === 0
    ) {
      this.lodgeGrievanceForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const request: CreateGrievanceRequest = {
      departmentId: this.lodgeGrievanceForm.value.departmentId,
      categoryId: this.lodgeGrievanceForm.value.categoryId,
      description: this.lodgeGrievanceForm.value.description,
      attachmentUrl: null, // File upload would be handled separately in real implementation
    };

    this.grievanceService.lodgeGrievance(request).subscribe({
      next: (response) => {
        this.notificationService.showSuccess(
          `Grievance #${response.grievanceNumber} submitted successfully`
        );
        this.router.navigate(['/citizen/grievances', response.grievanceId]);
        this.isSubmitting = false;
      },
      error: (error) => {
        this.isSubmitting = false;
        if (error.status === 400) {
          const errorMessage =
            error.error?.message || 'Failed to submit grievance. Please check your information.';
          this.notificationService.showError(errorMessage);
        } else {
          this.notificationService.showError('An error occurred while submitting the grievance');
        }
      },
    });
  }

  shouldShowError(fieldName: string): boolean {
    const field = this.lodgeGrievanceForm.get(fieldName);
    return !!(field && (field.touched || this.submitAttempted) && field.invalid);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.lodgeGrievanceForm.get(fieldName);
    if (!field || !field.errors) return '';

    const errors = field.errors;
    if (errors['required']) {
      if (fieldName === 'departmentId') return 'Please select a department';
      if (fieldName === 'categoryId') return 'Please select a category';
      if (fieldName === 'description') return 'Grievance description is required';
    }
    if (errors['minlength']) {
      return 'Description must be at least 50 characters';
    }
    if (errors['maxlength']) {
      return 'Description cannot exceed 2000 characters';
    }
    if (errors['fileType']) {
      return 'Only JPG, PNG, and PDF files are allowed';
    }
    if (errors['fileSize']) {
      return 'File size cannot exceed 5MB';
    }

    return 'Invalid value';
  }

  get descriptionLength(): number {
    return this.lodgeGrievanceForm.get('description')?.value?.length || 0;
  }
}
