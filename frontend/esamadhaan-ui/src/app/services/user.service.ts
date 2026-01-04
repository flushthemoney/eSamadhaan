import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { UserDto, CreateUserRequest, UpdateUserRequest, UpdateUserStatusRequest } from '../models/user';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAllUsers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(this.apiUrl);
  }

  getUserById(userId: number): Observable<UserDto> {
    return this.http.get<UserDto>(`${this.apiUrl}/${userId}`);
  }

  createUser(request: CreateUserRequest): Observable<UserDto> {
    return this.http.post<UserDto>(this.apiUrl, request);
  }

  updateUser(userId: number, request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${userId}`, request);
  }

  updateUserStatus(userId: number, request: UpdateUserStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${userId}/status`, request);
  }
}

