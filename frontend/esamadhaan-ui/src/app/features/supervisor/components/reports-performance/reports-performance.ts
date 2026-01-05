import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';

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
    FormsModule,
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
  filterType: 'all' | 'department' | 'category' = 'all';
  selectedDepartmentId: number | null = null;
  selectedCategoryId: number | null = null;

  constructor(
    private reportService: ReportService,
    private departmentService: DepartmentService,
    private categoryService: CategoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
    this.loadPerformanceReport();
  }

  loadDepartments(): void {
    this.departmentService.getAllDepartments().subscribe({
      next: (data) => {
        this.departments = data;
      },
      error: () => {},
    });
  }

  loadCategories(departmentId: number): void {
    this.categoryService.getCategoriesByDepartment(departmentId).subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: () => {},
    });
  }

  onFilterTypeChange(): void {
    this.selectedDepartmentId = null;
    this.selectedCategoryId = null;
    this.categories = [];
    this.loadPerformanceReport();
  }

  onDepartmentChange(): void {
    this.selectedCategoryId = null;
    this.categories = [];
    if (this.selectedDepartmentId) {
      this.loadCategories(this.selectedDepartmentId);
    }
    this.loadPerformanceReport();
  }

  loadPerformanceReport(): void {
    this.isLoading = true;
    let obs;
    if (this.filterType === 'department' && this.selectedDepartmentId) {
      obs = this.reportService.getResolutionTimeReport(this.selectedDepartmentId, undefined);
    } else if (this.filterType === 'category' && this.selectedCategoryId) {
      obs = this.reportService.getResolutionTimeReport(undefined, this.selectedCategoryId);
    } else {
      obs = this.reportService.getResolutionTimeReport();
    }

    obs.subscribe({
      next: (data) => {
        setTimeout(() => {
          this.performanceData = data;
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
}
