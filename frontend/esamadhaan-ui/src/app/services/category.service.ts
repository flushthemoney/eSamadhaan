import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CategoryDto } from '../models/category';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private readonly apiUrl = `${environment.apiUrl}`;

  constructor(private http: HttpClient) {}

  getCategoriesByDepartment(departmentId: number): Observable<CategoryDto[]> {
    return this.http.get<CategoryDto[]>(
      `${this.apiUrl}/department/${departmentId}/categories`
    );
  }
}

