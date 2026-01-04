import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { GrievanceService } from '../../../../services/grievance.service';
import { DepartmentService } from '../../../../services/department.service';
import { CategoryService } from '../../../../services/category.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { EmptyStateComponent } from '../../../../shared/components/empty-state/empty-state';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { GrievanceStatusBadgeComponent } from '../../../../shared/components/grievance-status-badge/grievance-status-badge';
import { GrievanceStatus } from '../../../../models/common';
import { DepartmentDto } from '../../../../models/department';
import { CategoryDto } from '../../../../models/category';
import { RelativeTimePipe } from '../../../../shared/pipes/relative-time-pipe';

@Component({
  selector: 'app-grievance-monitor',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    MatCardModule,
    MatTableModule,
    MatPaginatorModule,
    MatSelectModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatAutocompleteModule,
    MatProgressSpinnerModule,
    LoadingSpinnerComponent,
    EmptyStateComponent,
    PageHeaderComponent,
    GrievanceStatusBadgeComponent,
    RelativeTimePipe,
  ],
  templateUrl: './grievance-monitor.html',
  styleUrl: './grievance-monitor.scss',
})
export class GrievanceMonitorComponent implements OnInit {
  displayedColumns: string[] = ['grievanceNumber', 'departmentName', 'categoryName', 'status', 'createdAt'];
  grievances: any[] = [];
  filteredGrievances: any[] = [];
  departments: DepartmentDto[] = [];
  categories: CategoryDto[] = [];
  isLoading = false;
  isSearching = false;
  searchForm: FormGroup;
  pageSize = 25;
  pageIndex = 0;
  GrievanceStatus = GrievanceStatus;

  constructor(
    private grievanceService: GrievanceService,
    private departmentService: DepartmentService,
    private categoryService: CategoryService,
    private fb: FormBuilder
  ) {
    this.searchForm = this.fb.group({
      grievanceNumber: [''],
      departmentId: [''],
      categoryId: [''],
      status: [''],
      fromDate: [''],
      toDate: [''],
    });
  }

  ngOnInit(): void {
    this.loadDepartments();
    this.loadAllGrievances();

    this.searchForm.get('departmentId')?.valueChanges.subscribe((departmentId) => {
      this.searchForm.patchValue({ categoryId: '' });
      this.categories = [];
      if (departmentId) {
        this.loadCategories(departmentId);
      }
    });
  }

  loadAllGrievances(): void {
    this.isLoading = true;
    this.grievanceService.getAllGrievances().subscribe({
      next: (data) => {
        this.grievances = data;
        this.applyPagination();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
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

  loadCategories(departmentId: number): void {
    this.categoryService.getCategoriesByDepartment(departmentId).subscribe({
      next: (data) => {
        this.categories = data;
      },
      error: () => {},
    });
  }

  onSearch(): void {
    this.isSearching = true;
    const searchCriteria: any = {};
    const formValue = this.searchForm.value;

    if (formValue.grievanceNumber) searchCriteria.grievanceNumber = formValue.grievanceNumber;
    if (formValue.departmentId) searchCriteria.departmentId = formValue.departmentId;
    if (formValue.categoryId) searchCriteria.categoryId = formValue.categoryId;
    if (formValue.status) searchCriteria.status = formValue.status;
    if (formValue.fromDate) {
      searchCriteria.fromDate = formValue.fromDate instanceof Date 
        ? formValue.fromDate.toISOString().split('T')[0]
        : formValue.fromDate;
    }
    if (formValue.toDate) {
      searchCriteria.toDate = formValue.toDate instanceof Date
        ? formValue.toDate.toISOString().split('T')[0]
        : formValue.toDate;
    }

    this.grievanceService.searchGrievances(searchCriteria).subscribe({
      next: (data) => {
        this.grievances = data;
        this.pageIndex = 0;
        this.applyPagination();
        this.isSearching = false;
      },
      error: () => {
        this.isSearching = false;
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
    this.filteredGrievances = this.grievances.slice(start, end);
  }
}
