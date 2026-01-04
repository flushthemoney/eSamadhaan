import { Routes } from '@angular/router';
import { OfficerLayoutComponent } from './layout/officer-layout/officer-layout';
import { DashboardComponent } from './components/dashboard/dashboard';
import { QueueComponent } from './components/queue/queue';
import { DepartmentGrievancesComponent } from './components/department-grievances/department-grievances';
import { CategoryManagementComponent } from './components/category-management/category-management';
import { GrievanceDetailComponent } from './components/grievance-detail/grievance-detail';
import { ChangePasswordComponent } from '../../features/auth/components/change-password/change-password';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const officerRoutes: Routes = [
  {
    path: '',
    component: OfficerLayoutComponent,
    canActivate: [authGuard, roleGuard('DepartmentOfficer')],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'queue', component: QueueComponent },
      { path: 'department-grievances', component: DepartmentGrievancesComponent },
      { path: 'categories', component: CategoryManagementComponent },
      { path: 'grievances/:id', component: GrievanceDetailComponent },
      { path: 'change-password', component: ChangePasswordComponent },
    ],
  },
];

