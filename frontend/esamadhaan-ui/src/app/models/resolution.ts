// Resolution Interfaces

// Request DTOs
export interface CreateResolutionRequest {
  resolutionRemarks: string;
}

// Response DTOs
export interface ResolutionResponseDto {
  id: number;
  grievanceId: number;
  grievanceNumber: string;
  resolvedByOfficerId: number;
  resolvedByOfficerName: string;
  resolutionRemarks: string;
  resolvedAt: string; // ISO 8601 DateTime
}

