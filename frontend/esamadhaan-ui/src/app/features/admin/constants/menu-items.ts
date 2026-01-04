import { MenuItem } from '../../../shared/components/navigation-menu/navigation-menu';

export const ADMIN_MENU: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: 'dashboard',
    route: '/admin/dashboard',
  },
  {
    label: 'Departments',
    icon: 'business',
    route: '/admin/departments',
  },
  {
    label: 'Categories',
    icon: 'category',
    route: '/admin/categories',
  },
  {
    label: 'Users',
    icon: 'people',
    route: '/admin/users',
  },
  {
    label: 'Grievances',
    icon: 'view_list',
    route: '/admin/grievances',
  },
  {
    label: 'Reports',
    icon: 'assessment',
    route: '/admin/reports',
  },
  { type: 'divider' as const, label: '', icon: '' },
  {
    label: 'Change Password',
    icon: 'lock',
    route: '/admin/change-password',
  },
  {
    label: 'Logout',
    icon: 'logout',
    action: 'logout',
  },
];

