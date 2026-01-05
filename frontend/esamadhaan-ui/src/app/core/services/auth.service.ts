import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { BehaviorSubject, Observable } from "rxjs";
import { map, tap } from "rxjs/operators";
import { Router } from "@angular/router";
import { environment } from "../../../environments/environment";
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ProfileResponse,
  ChangePasswordRequest,
  EmailCheckResponse,
} from "../../models/auth";

@Injectable({
  providedIn: "root",
})
export class AuthService {
  private readonly TOKEN_KEY = "auth_token";
  private readonly apiUrl = environment.apiUrl;

  private currentUserSubject = new BehaviorSubject<ProfileResponse | null>(
    null
  );
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {
    // Load user from token on service init
    if (this.getToken()) {
      this.loadUserProfile();
    }
  }

  get currentUserValue(): ProfileResponse | null {
    return this.currentUserSubject.value;
  }

  get isAuthenticated(): boolean {
    return !!this.getToken() && !this.isTokenExpired();
  }

  get userRole(): string | null {
    const token = this.getToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    if (!payload) return null;

    // Handle both short form and full claim type URI
    // ClaimTypes.Role can be stored as "role" or "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    return payload.role || 
           payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ||
           payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] ||
           null;
  }

  get userId(): number | null {
    const token = this.getToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    return payload?.sub ? parseInt(payload.sub, 10) : null;
  }

  get departmentId(): number | null {
    const token = this.getToken();
    if (!token) return null;

    const payload = this.decodeToken(token);
    return payload?.departmentId ? parseInt(payload.departmentId, 10) : null;
  }

  login(credentials: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/auth/login`, credentials)
      .pipe(
        tap((response) => {
          this.setToken(response.token);
          this.loadUserProfile();
        })
      );
  }

  register(data: RegisterRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/register`, data);
  }

  logout(): void {
    this.removeToken();
    this.currentUserSubject.next(null);
    this.router.navigate(["/auth/login"]);
  }

  getProfile(): Observable<ProfileResponse> {
    return this.http.get<ProfileResponse>(`${this.apiUrl}/auth/profile`);
  }

  changePassword(data: ChangePasswordRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/auth/change-password`, data);
  }

  checkEmailAvailability(email: string): Observable<boolean> {
    return this.http
      .get<EmailCheckResponse>(
        `${this.apiUrl}/auth/check-email`,
        {
          params: { email },
        }
      )
      .pipe(map((response) => response.isAvailable));
  }

  private loadUserProfile(): void {
    this.getProfile().subscribe({
      next: (user) => this.currentUserSubject.next(user),
      error: () => this.logout(),
    });
  }

  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  private removeToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
  }

  private decodeToken(token: string): any {
    try {
      const payload = token.split(".")[1];
      return JSON.parse(atob(payload));
    } catch (error) {
      console.error("Error decoding token:", error);
      return null;
    }
  }

  isTokenExpired(): boolean {
    const token = this.getToken();
    if (!token) return true;

    const payload = this.decodeToken(token);
    if (!payload || !payload.exp) return true;

    const expirationDate = new Date(payload.exp * 1000);
    return expirationDate < new Date();
  }

  navigateByRole(): void {
    const role = this.userRole;
    if (!role) {
      console.warn('No role found in token, redirecting to home');
      this.router.navigate(["/"]);
      return;
    }

    const routes: Record<string, string> = {
      Citizen: "/citizen/dashboard",
      DepartmentOfficer: "/officer/dashboard",
      SupervisoryOfficer: "/supervisor/dashboard",
      SystemAdmin: "/admin/dashboard",
    };

    const route = routes[role] || "/";
    console.log(`Navigating to ${route} for role: ${role}`);
    this.router.navigate([route]);
  }
}

