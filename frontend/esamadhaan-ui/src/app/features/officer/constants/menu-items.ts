import { MenuItem } from '../../../shared/components/navigation-menu/navigation-menu';

export const OFFICER_MENU: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: 'dashboard',
    route: '/officer/dashboard',
  },
  {
    label: 'My Queue',
    icon: 'inbox',
    route: '/officer/queue',
  },
  {
    label: 'Department Grievances',
    icon: 'list_alt',
    route: '/officer/department-grievances',
  },
  {
    label: 'Categories',
    icon: 'category',
    route: '/officer/categories',
  },
  { type: 'divider' as const, label: '', icon: '' },
  {
    label: 'Change Password',
    icon: 'lock',
    route: '/officer/change-password',
  },
  {
    label: 'Logout',
    icon: 'logout',
    action: 'logout',
  },
];

