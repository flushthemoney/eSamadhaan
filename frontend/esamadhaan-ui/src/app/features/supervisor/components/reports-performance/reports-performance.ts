import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';

import { ReportService } from '../../../../services/report.service';
import { DepartmentService } from '../../../../services/department.service';
import { CategoryService } from '../../../../services/category.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { DepartmentDto } from '../../../../models/department';
import { CategoryDto } from '../../../../models/category';

@Component({
  selector: 'app-reports-performance',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
  ],
  templateUrl: './reports-performance.html',
  styleUrl: './reports-performance.scss',
})
export class ReportsPerformanceComponent implements OnInit {
  performanceData: any = null;
  departments: DepartmentDto[] = [];
  categories: CategoryDto[] = [];
  isLoading = false;
  filterForm: FormGroup;

  constructor(
    private reportService: ReportService,
    private departmentService: DepartmentService,
    private categoryService: CategoryService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef
  ) {
    this.filterForm = this.fb.group({
      departmentId: [''],
      categoryId: [{value: '', disabled: true}],
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadPerformanceReport();

    // Subscribe to department changes
    this.filterForm.get('departmentId')?.valueChanges.subscribe((departmentId) => {
      const categoryControl = this.filterForm.get('categoryId');
      categoryControl?.patchValue('');
      this.categories = [];
      if (departmentId) {
        categoryControl?.enable();
        this.loadCategories(departmentId);
      } else {
        categoryControl?.disable();
      }
      this.loadPerformanceReport();
    });

    // Subscribe to category changes
    this.filterForm.get('categoryId')?.valueChanges.subscribe(() => {
      this.loadPerformanceReport();
    });
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

  loadCategories(departmentId: number): void {
    this.categoryService.getCategoriesByDepartment(departmentId).subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.categories = data;
          this.cdr.markForCheck();
        }, 0);
      },
      error: () => {},
    });
  }

  loadPerformanceReport(): void {
    this.isLoading = true;
    const departmentId = this.filterForm.get('departmentId')?.value;
    const categoryId = this.filterForm.get('categoryId')?.value;

    let obs;
    if (categoryId) {
      // Department + Category: use category endpoint
      obs = this.reportService.getResolutionTimeReport(undefined, categoryId);
    } else if (departmentId) {
      // Department only: use department endpoint
      obs = this.reportService.getResolutionTimeReport(departmentId, undefined);
    } else {
      // No filters: use general endpoint
      obs = this.reportService.getResolutionTimeReport();
    }

    obs.subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.performanceData = data;
          this.isLoading = false;
          this.cdr.markForCheck();
        }, 0);
      },
      error: () => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        }, 0);
      },
    });
  }
}
