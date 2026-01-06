import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { FormsModule } from '@angular/forms';

import { ReportService } from '../../../../services/report.service';
import { DepartmentService } from '../../../../services/department.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { DepartmentDto } from '../../../../models/department';

@Component({
  selector: 'app-reports-officer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
  ],
  templateUrl: './reports-officer.html',
  styleUrl: './reports-officer.scss',
})
export class ReportsOfficerComponent implements OnInit {
  displayedColumns: string[] = ['officerName', 'departmentName', 'totalAssigned', 'resolved', 'pending', 'resolutionRate', 'avgResolutionTime'];
  officerPerformance: any[] = [];
  dataSource = new MatTableDataSource<any>([]);
  isLoading = false;
  topCount: number = 10;
  departments: DepartmentDto[] = [];
  selectedDepartmentId: number | null = null;

  constructor(
    private reportService: ReportService,
    private departmentService: DepartmentService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
    this.loadOfficerPerformance();
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

  loadOfficerPerformance(): void {
    this.isLoading = true;
    this.reportService.getOfficerPerformanceReport(this.topCount, this.selectedDepartmentId || undefined).subscribe({
      next: (data) => {
        // Defer state changes to avoid change detection errors
        setTimeout(() => {
          this.officerPerformance = data;
          this.dataSource.data = data;
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
  

  onTopCountChange(): void {
    this.loadOfficerPerformance();
  }

  onDepartmentChange(): void {
    this.loadOfficerPerformance();
  }
}
