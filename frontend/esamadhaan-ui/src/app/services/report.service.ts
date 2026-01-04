import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  private readonly apiUrl = `${environment.apiUrl}/grievance/reports`;

  constructor(private http: HttpClient) {}

  getStatusReport(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/status`);
  }

  getResolutionTimeReport(departmentId?: number, categoryId?: number): Observable<any> {
    let url = `${this.apiUrl}/resolution-time`;
    if (departmentId) {
      url = `${this.apiUrl}/resolution-time/department/${departmentId}`;
    } else if (categoryId) {
      url = `${this.apiUrl}/resolution-time/category/${categoryId}`;
    }
    return this.http.get<any>(url);
  }

  getDepartmentPerformanceReport(departmentId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/department/${departmentId}/performance`);
  }

  getOfficerPerformanceReport(topCount?: number): Observable<any[]> {
    let params = new HttpParams();
    if (topCount) {
      params = params.set('topCount', topCount.toString());
    }
    return this.http.get<any[]>(`${this.apiUrl}/officers/top-performers`, { params });
  }

  getFeedbackAnalytics(departmentId?: number): Observable<any> {
    let url = `${this.apiUrl}/feedback/analytics`;
    if (departmentId) {
      url = `${this.apiUrl}/feedback/analytics/department/${departmentId}`;
    }
    return this.http.get<any>(url);
  }
}

