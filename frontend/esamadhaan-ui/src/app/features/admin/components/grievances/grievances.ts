import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { GrievanceMonitorComponent } from '../../../supervisor/components/grievance-monitor/grievance-monitor';

@Component({
  selector: 'app-admin-grievances',
  standalone: true,
  imports: [CommonModule, RouterModule, GrievanceMonitorComponent],
  template: `<app-grievance-monitor></app-grievance-monitor>`,
  styleUrl: './grievances.scss',
})
export class AdminGrievancesComponent {}
