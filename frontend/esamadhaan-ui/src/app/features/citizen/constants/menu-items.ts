import { MenuItem } from '../../../shared/components/navigation-menu/navigation-menu';

export const CITIZEN_MENU: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: 'dashboard',
    route: '/citizen/dashboard',
  },
  {
    label: 'My Grievances',
    icon: 'list_alt',
    route: '/citizen/grievances',
  },
  {
    label: 'Lodge Grievance',
    icon: 'add_circle',
    route: '/citizen/grievances/new',
  },
  {
    label: 'Feedback',
    icon: 'feedback',
    route: '/citizen/feedback',
  },
  { type: 'divider' as const, label: '', icon: '' },
  {
    label: 'Change Password',
    icon: 'lock',
    route: '/citizen/change-password',
  },
  {
    label: 'Logout',
    icon: 'logout',
    action: 'logout',
  },
];

