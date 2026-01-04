import { Routes } from '@angular/router';
import { SupervisorLayoutComponent } from './layout/supervisor-layout/supervisor-layout';
import { SupervisorDashboardComponent } from './components/dashboard/dashboard';
import { GrievanceMonitorComponent } from './components/grievance-monitor/grievance-monitor';
import { EscalationsComponent } from './components/escalations/escalations';
import { ReportsComponent } from './components/reports/reports';
import { ReportsOverviewComponent } from './components/reports-overview/reports-overview';
import { ReportsPerformanceComponent } from './components/reports-performance/reports-performance';
import { ReportsOfficerComponent } from './components/reports-officer/reports-officer';
import { ReportsFeedbackComponent } from './components/reports-feedback/reports-feedback';
import { ChangePasswordComponent } from '../../features/auth/components/change-password/change-password';
import { authGuard } from '../../core/guards/auth.guard';
import { roleGuard } from '../../core/guards/role.guard';

export const supervisorRoutes: Routes = [
  {
    path: '',
    component: SupervisorLayoutComponent,
    canActivate: [authGuard, roleGuard('SupervisoryOfficer')],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: SupervisorDashboardComponent },
      { path: 'grievances', component: GrievanceMonitorComponent },
      { path: 'escalations', component: EscalationsComponent },
      {
        path: 'reports',
        component: ReportsComponent,
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

