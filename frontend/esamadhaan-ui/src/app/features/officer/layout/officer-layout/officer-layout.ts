import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, NavigationEnd } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatDividerModule } from '@angular/material/divider';
import { NavigationMenuComponent, MenuItem } from '../../../../shared/components/navigation-menu/navigation-menu';
import { AuthService } from '../../../../core/services/auth.service';
import { OFFICER_MENU } from '../../constants/menu-items';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-officer-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatListModule,
    MatDividerModule,
    NavigationMenuComponent,
  ],
  templateUrl: './officer-layout.html',
  styleUrl: './officer-layout.scss',
})
export class OfficerLayoutComponent implements OnInit {
  menuItems: MenuItem[] = OFFICER_MENU;
  sidenavOpened = true;
  currentUser$;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this.currentUser$ = this.authService.currentUser$;
  }

  ngOnInit(): void {
    // Close sidenav on mobile after navigation
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        if (window.innerWidth < 960) {
          this.sidenavOpened = false;
        }
      });
  }

  toggleSidenav(): void {
    this.sidenavOpened = !this.sidenavOpened;
  }

  handleMenuAction(action: string): void {
    if (action === 'logout') {
      this.authService.logout();
    }
  }
}
