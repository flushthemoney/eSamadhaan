import { inject } from "@angular/core";
import { Router, CanActivateFn, ActivatedRouteSnapshot } from "@angular/router";
import { AuthService } from "../services/auth.service";

export const roleGuard = (requiredRole: string): CanActivateFn => {
  return (route: ActivatedRouteSnapshot): boolean => {
    const authService = inject(AuthService);
    const router = inject(Router);

    const userRole = authService.userRole;

    if (!authService.isAuthenticated || !userRole) {
      router.navigate(["/auth/login"]);
      return false;
    }

    if (userRole === requiredRole) {
      return true;
    }

    // User has valid token but wrong role
    router.navigate(["/unauthorized"]);
    return false;
  };
};

