import { Injectable } from "@angular/core";
import { MatSnackBar } from "@angular/material/snack-bar";
import { environment } from "../../../environments/environment";

@Injectable({
  providedIn: "root",
})
export class NotificationService {
  constructor(private snackBar: MatSnackBar) {}

  showSuccess(message: string, duration?: number): void {
    this.snackBar.open(message, "Close", {
      duration: duration ?? environment.notification.successDuration,
      horizontalPosition: environment.notification.position.horizontal,
      verticalPosition: environment.notification.position.vertical,
      panelClass: ["success-snackbar"],
    });
  }

  showError(message: string, duration?: number): void {
    this.snackBar.open(message, "Close", {
      duration: duration ?? environment.notification.errorDuration,
      horizontalPosition: environment.notification.position.horizontal,
      verticalPosition: environment.notification.position.vertical,
      panelClass: ["error-snackbar"],
    });
  }

  showWarning(message: string, duration?: number): void {
    this.snackBar.open(message, "Close", {
      duration: duration ?? environment.notification.warningDuration,
      horizontalPosition: environment.notification.position.horizontal,
      verticalPosition: environment.notification.position.vertical,
      panelClass: ["warning-snackbar"],
    });
  }

  showInfo(message: string, duration?: number): void {
    this.snackBar.open(message, "Close", {
      duration: duration ?? environment.notification.infoDuration,
      horizontalPosition: environment.notification.position.horizontal,
      verticalPosition: environment.notification.position.vertical,
      panelClass: ["info-snackbar"],
    });
  }
}

