import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError } from "rxjs";
import { Router } from "@angular/router";
import { AuthService } from "../services/auth.service";
import { NotificationService } from "../services/notification.service";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const notificationService = inject(NotificationService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        // Unauthorized - token expired or invalid
        authService.logout();
        notificationService.showWarning(
          "Your session has expired. Please log in again."
        );
        router.navigate(["/auth/login"]);
      } else if (error.status === 403) {
        // Forbidden - insufficient permissions
        router.navigate(["/unauthorized"]);
      } else if (error.status === 404) {
        // Not found - skip notification for resolution endpoint (resolution may not exist yet)
        if (!req.url.includes('/resolution')) {
          notificationService.showError("Resource not found");
        }
      } else if (error.status === 0) {
        // Network error
        if (!navigator.onLine) {
          notificationService.showError(
            "No internet connection. Please check your network."
          );
        } else {
          notificationService.showError(
            "Connection failed. Please try again."
          );
        }
      } else if (error.status >= 500) {
        // Server error
        notificationService.showError(
          "An unexpected error occurred. Please try again later."
        );
      }
      // Note: 400 errors are typically handled by components to show field-specific errors

      return throwError(() => error);
    })
  );
};

