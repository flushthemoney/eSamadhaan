import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateGrievanceRequest,
  GrievanceResponseDto,
  GrievanceListDto,
  LodgeGrievanceResponse,
  GrievanceStatusHistoryDto,
  ResolutionDto,
} from '../models/grievance';

@Injectable({
  providedIn: 'root',
})
export class GrievanceService {
  private readonly apiUrl = `${environment.apiUrl}/grievance`;

  constructor(private http: HttpClient) {}

  // CITIZEN APIs
  getMyGrievances(status?: number): Observable<GrievanceListDto[]> {
    let params = new HttpParams();
    if (status) {
      params = params.set('status', status.toString());
    }
    return this.http.get<GrievanceListDto[]>(`${this.apiUrl}/my-grievances`, {
      params,
    });
  }

  lodgeGrievance(
    request: CreateGrievanceRequest
  ): Observable<LodgeGrievanceResponse> {
    return this.http.post<LodgeGrievanceResponse>(
      `${this.apiUrl}/lodge`,
      request
    );
  }

  getGrievanceById(id: number): Observable<GrievanceResponseDto> {
    return this.http.get<GrievanceResponseDto>(`${this.apiUrl}/${id}`);
  }

  getGrievanceHistory(grievanceId: number): Observable<GrievanceStatusHistoryDto[]> {
    return this.http.get<GrievanceStatusHistoryDto[]>(
      `${this.apiUrl}/${grievanceId}/history`
    );
  }

  getGrievanceResolution(grievanceId: number): Observable<ResolutionDto> {
    return this.http.get<ResolutionDto>(
      `${this.apiUrl}/${grievanceId}/resolution`
    );
  }

  canEscalate(grievanceId: number): Observable<{ canEscalate: boolean }> {
    return this.http.get<{ canEscalate: boolean }>(
      `${this.apiUrl}/${grievanceId}/can-escalate`
    );
  }

  escalateGrievance(grievanceId: number, reason: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${grievanceId}/escalate`, {
      reason,
    });
  }
}

