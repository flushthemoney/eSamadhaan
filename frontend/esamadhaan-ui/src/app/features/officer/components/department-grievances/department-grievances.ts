import { Component, OnInit, ChangeDetectorRef, AfterViewInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';

import { GrievanceService } from '../../../../services/grievance.service';
import { CategoryService } from '../../../../services/category.service';
import { AuthService } from '../../../../core/services/auth.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceStatus } from '../../../../models/common';
import { CategoryDto } from '../../../../models/category';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-department-grievances',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './department-grievances.html',
  styleUrl: './department-grievances.scss',
})
export class DepartmentGrievancesComponent implements OnInit, AfterViewInit {
  displayedColumns: string[] = ['grievanceNumber', 'categoryName', 'status', 'createdAt', 'actions'];
  grievances: any[] = [];
  dataSource = new MatTableDataSource<any>([]);
  categories: CategoryDto[] = [];
  isLoading = false;
  isLoadingCategories = false;
  selectedStatus: number | null = null;
  selectedCategoryId: number | null = null;
  selectedSortBy: string = '';
  GrievanceStatus = GrievanceStatus;
  departmentId: number | null = null;
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;

  constructor(
    private grievanceService: GrievanceService,
    private categoryService: CategoryService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) {
    this.departmentId = this.authService.departmentId;
  }

  ngOnInit(): void {
    if (this.departmentId) {
      this.loadCategories();
      this.loadGrievances();
    }
  }

  loadCategories(): void {
    if (!this.departmentId) return;
    this.isLoadingCategories = true;
    this.categoryService.getCategoriesByDepartment(this.departmentId).subscribe({
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

  loadGrievances(): void {
    this.isLoading = true;
    const filters: any = {};
    if (this.selectedStatus) filters.status = this.selectedStatus;
    if (this.selectedCategoryId) filters.categoryId = this.selectedCategoryId;
    if (this.selectedSortBy) filters.sortBy = this.selectedSortBy;

    this.grievanceService.getDepartmentGrievances(filters).subscribe({
      next: (data) => {
        setTimeout(() => {
          this.grievances = data;
          this.dataSource.data = this.grievances;
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

  onFilterChange(): void {
    this.loadGrievances();
  }
}
