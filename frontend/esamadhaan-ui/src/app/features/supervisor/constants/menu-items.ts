import { MenuItem } from '../../../shared/components/navigation-menu/navigation-menu';

export const SUPERVISOR_MENU: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: 'dashboard',
    route: '/supervisor/dashboard',
  },
  {
    label: 'All Grievances',
    icon: 'view_list',
    route: '/supervisor/grievances',
  },
  {
    label: 'Escalations',
    icon: 'priority_high',
    route: '/supervisor/escalations',
  },
  {
    label: 'Reports',
    icon: 'assessment',
    route: '/supervisor/reports',
  },
  { type: 'divider' as const, label: '', icon: '' },
  {
    label: 'Change Password',
    icon: 'lock',
    route: '/supervisor/change-password',
  },
  {
    label: 'Logout',
    icon: 'logout',
    action: 'logout',
  },
];

