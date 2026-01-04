import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { FormsModule } from '@angular/forms';

import { ReportService } from '../../../../services/report.service';
import { LoadingSpinnerComponent } from '../../../../shared/components/loading-spinner/loading-spinner';
import { PageHeaderComponent } from '../../../../shared/components/page-header/page-header';

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
    LoadingSpinnerComponent,
    PageHeaderComponent,
  ],
  templateUrl: './reports-officer.html',
  styleUrl: './reports-officer.scss',
})
export class ReportsOfficerComponent implements OnInit {
  displayedColumns: string[] = ['officerName', 'departmentName', 'totalAssigned', 'resolved', 'pending', 'resolutionRate', 'avgResolutionTime'];
  officerPerformance: any[] = [];
  isLoading = false;
  topCount: number = 10;

  constructor(private reportService: ReportService) {}

  ngOnInit(): void {
    this.loadOfficerPerformance();
  }

  loadOfficerPerformance(): void {
    this.isLoading = true;
    this.reportService.getOfficerPerformanceReport(this.topCount).subscribe({
      next: (data) => {
        this.officerPerformance = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
    });
  }

  onTopCountChange(): void {
    this.loadOfficerPerformance();
  }
}
