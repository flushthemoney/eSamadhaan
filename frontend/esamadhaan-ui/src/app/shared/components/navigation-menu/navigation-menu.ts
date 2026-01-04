import { Component, Input } from '@angular/core';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

export interface MenuItem {
  label: string;
  route?: string;
  icon: string;
  action?: string;
  badge?: string | number;
  type?: 'divider';
}

@Component({
  selector: 'app-navigation-menu',
  standalone: true,
  imports: [MatListModule, MatIconModule, RouterModule, CommonModule],
  templateUrl: './navigation-menu.html',
  styleUrl: './navigation-menu.scss',
})
export class NavigationMenuComponent {
  @Input() menuItems: MenuItem[] = [];

  handleLogout(event: Event): void {
    // This will be handled by parent component via event emitter
    // For now, prevent default navigation
    event.preventDefault();
  }
}
