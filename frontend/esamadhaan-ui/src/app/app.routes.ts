import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/components/login/login';
import { RegisterComponent } from './features/auth/components/register/register';
import { ChangePasswordComponent } from './features/auth/components/change-password/change-password';
import { UnauthorizedComponent } from './shared/components/unauthorized/unauthorized';
import { NotFoundComponent } from './shared/components/not-found/not-found';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
  
  // Public auth routes
  { path: 'auth/login', component: LoginComponent },
  { path: 'auth/register', component: RegisterComponent },
  { path: 'auth/change-password', component: ChangePasswordComponent },
  
  // Redirect legacy routes
  { path: 'login', redirectTo: '/auth/login', pathMatch: 'full' },
  { path: 'register', redirectTo: '/auth/register', pathMatch: 'full' },
  
  // Error pages
  { path: 'unauthorized', component: UnauthorizedComponent },
  
  // Feature module routes will be added in later phases
  // { path: 'citizen', loadChildren: () => import('./features/citizen/citizen.routes').then(m => m.routes) },
  // { path: 'officer', loadChildren: () => import('./features/officer/officer.routes').then(m => m.routes) },
  // { path: 'supervisor', loadChildren: () => import('./features/supervisor/supervisor.routes').then(m => m.routes) },
  // { path: 'admin', loadChildren: () => import('./features/admin/admin.routes').then(m => m.routes) },
  
  // 404 - must be last
  { path: '**', component: NotFoundComponent },
];
