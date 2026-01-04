import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { OfficerListDto } from '../models/user';

@Injectable({
  providedIn: 'root',
})
export class OfficerService {
  private readonly apiUrl = `${environment.apiUrl}/department`;

  constructor(private http: HttpClient) {}

  getDepartmentOfficers(): Observable<OfficerListDto[]> {
    return this.http.get<OfficerListDto[]>(`${this.apiUrl}/officers`);
  }
}

