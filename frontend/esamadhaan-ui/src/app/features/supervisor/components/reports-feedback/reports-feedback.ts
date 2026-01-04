import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from '@angular/material/table';
import { FormsModule } from '@angular/forms';

import { ReportService } from '../../../../services/report.service';
import { DepartmentService } from '../../../../services/department.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';
import { DepartmentDto } from '../../../../models/department';

@Component({
  selector: 'app-reports-feedback',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatCardModule,
    MatSelectModule,
    MatFormFieldModule,
    MatProgressSpinnerModule,
    MatTableModule,
    LoadingSpinnerComponent,
    PageHeaderComponent,
  ],
  templateUrl: './reports-feedback.html',
  styleUrl: './reports-feedback.scss',
})
export class ReportsFeedbackComponent implements OnInit {
  feedbackData: any = null;
  departments: DepartmentDto[] = [];
  isLoading = false;
  selectedDepartmentId: number | null = null;
  displayedColumns: string[] = ['rating', 'count', 'percentage'];

  constructor(
    private reportService: ReportService,
    private departmentService: DepartmentService
  ) {}

  ngOnInit(): void {
    this.loadDepartments();
    this.loadFeedbackAnalytics();
  }

  loadDepartments(): void {
    this.departmentService.getAllDepartments().subscribe({
      next: (data) => {
        this.departments = data;
      },
      error: () => {},
    });
  }

  onDepartmentChange(): void {
    this.loadFeedbackAnalytics();
  }

  loadFeedbackAnalytics(): void {
    this.isLoading = true;
    this.reportService.getFeedbackAnalytics(this.selectedDepartmentId || undefined).subscribe({
      next: (data) => {
        this.feedbackData = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  getRatingDistribution(): any[] {
    if (!this.feedbackData) return [];
    return [1, 2, 3, 4, 5].map((rating) => ({
      rating,
      count: this.feedbackData.feedbackCountByRating?.[rating] || 0,
      percentage: this.feedbackData.ratingPercentages?.[rating] || 0,
    }));
  }
}
