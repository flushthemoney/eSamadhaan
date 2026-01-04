import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, RouterModule, MatTabsModule],
  template: `
    <mat-tab-group [selectedIndex]="selectedIndex" (selectedIndexChange)="onTabChange($event)">
      <mat-tab label="Overview"></mat-tab>
      <mat-tab label="Performance"></mat-tab>
      <mat-tab label="Officer Performance"></mat-tab>
      <mat-tab label="Feedback Analytics"></mat-tab>
    </mat-tab-group>
    <router-outlet></router-outlet>
  `,
  styleUrl: './reports.scss',
})
export class ReportsComponent {
  selectedIndex = 0;
  routes = ['overview', 'performance', 'officer', 'feedback'];

  constructor(private router: Router, private route: ActivatedRoute) {
    this.updateSelectedIndex();
  }

  ngOnInit(): void {
    this.route.children[0]?.url.subscribe(segments => {
      this.updateSelectedIndex();
    });
  }

  updateSelectedIndex(): void {
    const url = this.router.url;
    const index = this.routes.findIndex(r => url.includes(r));
    if (index >= 0) {
      this.selectedIndex = index;
    }
  }

  onTabChange(index: number): void {
    this.router.navigate([this.routes[index]], { relativeTo: this.route });
  }
}
