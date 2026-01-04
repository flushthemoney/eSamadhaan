import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DepartmentDto } from '../models/department';

@Injectable({
  providedIn: 'root',
})
export class DepartmentService {
  private readonly apiUrl = `${environment.apiUrl}/department`;

  constructor(private http: HttpClient) {}

  getAllDepartments(): Observable<DepartmentDto[]> {
    return this.http.get<DepartmentDto[]>(this.apiUrl);
  }

  getDepartmentById(id: number): Observable<DepartmentDto> {
    return this.http.get<DepartmentDto>(`${this.apiUrl}/${id}`);
  }

  getDepartmentOfficers(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/officers`);
  }
}

