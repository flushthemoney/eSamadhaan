import { Component, Input } from '@angular/core';
import { MatChipsModule } from '@angular/material/chips';
import { CommonModule } from '@angular/common';
import { GrievanceStatus, GrievanceStatusLabels } from '../../../models/common';

@Component({
  selector: 'app-grievance-status-badge',
  standalone: true,
  imports: [MatChipsModule, CommonModule],
  templateUrl: './grievance-status-badge.html',
  styleUrl: './grievance-status-badge.scss',
})
export class GrievanceStatusBadgeComponent {
  @Input() status!: GrievanceStatus;

  get statusLabel(): string {
    return GrievanceStatusLabels[this.status] || 'Unknown';
  }

  get statusClass(): string {
    const classes: Record<GrievanceStatus, string> = {
      [GrievanceStatus.Submitted]: 'submitted',
      [GrievanceStatus.Assigned]: 'assigned',
      [GrievanceStatus.InReview]: 'in-review',
      [GrievanceStatus.Resolved]: 'resolved',
      [GrievanceStatus.Closed]: 'closed',
    };
    return classes[this.status] || 'unknown';
  }
}
