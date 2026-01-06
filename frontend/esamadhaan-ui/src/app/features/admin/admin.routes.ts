import { Routes } from '@angular/router';
import { AdminLayoutComponent } from './layout/admin-layout/admin-layout';
import { AdminDashboardComponent } from './components/dashboard/dashboard';
import { DepartmentManagementComponent } from './components/department-management/department-management';
import { CategoryManagementComponent } from './components/category-management/category-management';
import { UserManagementComponent } from './components/user-management/user-management';
import { AdminGrievancesComponent } from './components/grievances/grievances';
import { AdminGrievanceDetailComponent } from './components/grievance-detail/grievance-detail';
import { AdminReportsComponent } from './components/reports/reports';
import { ReportsOverviewComponent } from './components/reports-overview/reports-overview';
import { ReportsPerformanceComponent } from './components/reports-performance/reports-performance';
import { ReportsOfficerComponent } from './components/reports-officer/reports-officer';
import { ReportsFeedbackComponent } from './components/reports-feedback/reports-feedback';
import { ChangePasswordComponent } from '../../features/auth/components/change-password/change-password';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const adminRoutes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard, roleGuard('SystemAdmin')],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: AdminDashboardComponent },
      { path: 'departments', component: DepartmentManagementComponent },
      { path: 'categories', component: CategoryManagementComponent },
      { path: 'users', component: UserManagementComponent },
      { path: 'grievances', component: AdminGrievancesComponent },
      { path: 'grievances/:id', component: AdminGrievanceDetailComponent },
      {
        path: 'reports',
        component: AdminReportsComponent,
        children: [
          { path: '', redirectTo: 'overview', pathMatch: 'full' },
          { path: 'overview', component: ReportsOverviewComponent },
          { path: 'performance', component: ReportsPerformanceComponent },
          { path: 'officer', component: ReportsOfficerComponent },
          { path: 'feedback', component: ReportsFeedbackComponent },
        ],
      },
      { path: 'change-password', component: ChangePasswordComponent },
    ],
  },
];

