// Assignment Interfaces
import { GrievanceStatus } from './common';

// Request DTOs
export interface CreateAssignmentRequest {
  officerId: number;
}

export interface ReassignmentRequest {
  newOfficerId: number;
  reason?: string | null;
}

// Response DTOs
export interface AssignmentResponseDto {
  id: number;
  grievanceId: number;
  grievanceNumber: string;
  categoryName: string;
  currentStatus: GrievanceStatus;
  createdAt: string; // ISO 8601 DateTime
}

// Legacy interface name for backward compatibility
export interface AssignGrievanceResponse extends AssignmentResponseDto {}

