// Feedback Interfaces

// Request DTOs
export interface CreateFeedbackRequest {
  rating: number; // 1-5
  comment?: string | null;
}

// Response DTOs
export interface FeedbackResponseDto {
  id: number;
  grievanceId: number;
  grievanceNumber: string;
  rating: number; // 1-5
  comment: string | null;
  submittedAt: string; // ISO 8601 DateTime
}

