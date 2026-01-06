import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';

import { GrievanceService } from '../../../../services/grievance.service';
import { DepartmentService } from '../../../../services/department.service';
import { CategoryService } from '../../../../services/category.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatus, GrievanceStatusLabels } from '../../../../models/common';
import { DepartmentDto } from '../../../../models/department';
import { CategoryDto } from '../../../../models/category';

@Component({
  selector: 'app-escalations',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatFormFieldModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
  ],
  templateUrl: './escalations.html',
  styleUrl: './escalations.scss',
})
export class EscalationsComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['grievanceNumber', 'departmentName', 'categoryName', 'status', 'daysSinceSubmission', 'actions'];
  escalations: any[] = [];
  allEscalations: any[] = [];
  dataSource = new MatTableDataSource<any>([]);
  isLoading = false;
  isLoadingDepartments = false;
  isLoadingCategories = false;
  departments: DepartmentDto[] = [];
  categories: CategoryDto[] = [];
  selectedDepartmentId: number | null = null;
  selectedCategoryId: number | null = null;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private grievanceService: GrievanceService,
    private departmentService: DepartmentService,
    private categoryService: CategoryService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
    this.loadEscalations();
  }

  loadDepartments(): void {
    this.isLoadingDepartments = true;
    this.departmentService.getAllDepartments().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.departments = data;
          this.isLoadingDepartments = false;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        setTimeout(() => {
          this.isLoadingDepartments = false;
          this.cdr.detectChanges();
        }, 0);
      },
    });
  }

  loadCategories(departmentId: number): void {
    this.isLoadingCategories = true;
    this.categoryService.getCategoriesByDepartment(departmentId).subscribe({
      next: (data) => {
        setTimeout(() => {
          this.categories = data;
          this.isLoadingCategories = false;
          this.cdr.detectChanges();
        }, 0);
      },
      error: () => {
        setTimeout(() => {
          this.isLoadingCategories = false;
          this.cdr.detectChanges();
        }, 0);
      },
    });
  }

  onFilterChange(): void {
    if (this.selectedDepartmentId) {
      this.loadCategories(this.selectedDepartmentId);
    } else {
      this.categories = [];
      this.selectedCategoryId = null;
    }
    this.applyFilters();
  }

  applyFilters(): void {
    let filtered = [...this.allEscalations];

    if (this.selectedDepartmentId) {
      const selectedDepartment = this.departments.find(d => d.id === this.selectedDepartmentId);
      if (selectedDepartment) {
        filtered = filtered.filter(e => e.departmentName === selectedDepartment.name);
      }
    }

    if (this.selectedCategoryId) {
      const selectedCategory = this.categories.find(c => c.id === this.selectedCategoryId);
      if (selectedCategory) {
        filtered = filtered.filter(e => e.categoryName === selectedCategory.name);
      }
    }

    this.escalations = filtered;
    this.dataSource.data = this.escalations;
    
    if (this.paginator) {
      this.paginator.pageIndex = 0;
    }
    this.cdr.detectChanges();
  }

  loadEscalations(): void {
    this.isLoading = true;
    this.grievanceService.getEscalatedGrievances().subscribe({
      next: (data) => {
        setTimeout(() => {
          this.allEscalations = data;
          this.escalations = data;
          this.dataSource.data = this.escalations;
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

  getStatusLabel(status: number): string {
    return GrievanceStatusLabels[status as GrievanceStatus] || 'Unknown';
  }

  hasActiveFilters(): boolean {
    return this.selectedDepartmentId !== null || this.selectedCategoryId !== null;
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
}
