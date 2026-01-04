// Assignment Interfaces

// Request DTOs
export interface CreateAssignmentRequest {
  officerId: number;
}

// Response DTOs
export interface AssignGrievanceResponse {
  id: number;
  grievanceId: number;
  grievanceNumber: string;
  assignedAt: string; // ISO 8601 DateTime
}

