import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: '/auth/login', pathMatch: 'full' },
  // Feature module routes will be added in later phases
  { path: '**', redirectTo: '/auth/login' }, // Temporary - will be replaced with 404 component
];
