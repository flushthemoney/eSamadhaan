import { Routes } from '@angular/router';
import { CitizenLayoutComponent } from './layout/citizen-layout/citizen-layout';
import { DashboardComponent } from './components/dashboard/dashboard';
import { LodgeGrievanceComponent } from './components/lodge-grievance/lodge-grievance';
import { GrievanceListComponent } from './components/grievance-list/grievance-list';
import { GrievanceDetailComponent } from './components/grievance-detail/grievance-detail';
import { FeedbackComponent } from './components/feedback/feedback';
import { ChangePasswordComponent } from '../../features/auth/components/change-password/change-password';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const citizenRoutes: Routes = [
  {
    path: '',
    component: CitizenLayoutComponent,
    canActivate: [authGuard, roleGuard('Citizen')],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'grievances', component: GrievanceListComponent },
      { path: 'grievances/new', component: LodgeGrievanceComponent },
      { path: 'grievances/:id', component: GrievanceDetailComponent },
      { path: 'feedback', component: FeedbackComponent },
      { path: 'change-password', component: ChangePasswordComponent },
    ],
  },
];

