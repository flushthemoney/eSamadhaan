import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { FeedbackResponseDto, CreateFeedbackRequest } from '../models/feedback';

@Injectable({
  providedIn: 'root',
})
export class FeedbackService {
  private readonly apiUrl = `${environment.apiUrl}/grievance`;

  constructor(private http: HttpClient) {}

  getFeedback(grievanceId: number): Observable<FeedbackResponseDto> {
    return this.http.get<FeedbackResponseDto>(
      `${this.apiUrl}/${grievanceId}/feedback`
    );
  }

  submitFeedback(
    grievanceId: number,
    request: CreateFeedbackRequest
  ): Observable<FeedbackResponseDto> {
    return this.http.post<FeedbackResponseDto>(
      `${this.apiUrl}/${grievanceId}/feedback`,
      request
    );
  }
}

