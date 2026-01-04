import { Component, Input, Output, EventEmitter } from '@angular/core';
import { MatListModule } from '@angular/material/list';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';

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
  imports: [MatListModule, MatIconModule, RouterModule, CommonModule, MatDividerModule],
  templateUrl: './navigation-menu.html',
  styleUrl: './navigation-menu.scss',
})
export class NavigationMenuComponent {
  @Input() menuItems: MenuItem[] = [];
  @Output() menuAction = new EventEmitter<string>();

  handleLogout(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.menuAction.emit('logout');
  }
}
