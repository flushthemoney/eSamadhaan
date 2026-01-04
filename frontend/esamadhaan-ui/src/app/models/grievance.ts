import { GrievanceStatus } from './common';

// Request DTOs
export interface CreateGrievanceRequest {
  categoryId: number;
  departmentId: number;
  description: string;
  attachmentUrl?: string | null;
}

export interface ChangeGrievanceStatusRequest {
  newStatus: GrievanceStatus;
  remarks?: string | null;
}

export interface EscalateGrievanceRequest {
  reason: string;
}

// Response DTOs
export interface GrievanceResponseDto {
  id: number;
  grievanceNumber: string;
  citizenId: number;
  categoryId: number;
  categoryName: string;
  departmentId: number;
  departmentName: string;
  currentStatus: GrievanceStatus;
  description: string;
  attachmentUrl: string | null;
  createdAt: string; // ISO 8601 DateTime
  updatedAt: string; // ISO 8601 DateTime
  currentAssignment: AssignmentDto | null;
  resolution: ResolutionDto | null;
  feedback: FeedbackDto | null;
}

export interface GrievanceListDto {
  id: number;
  grievanceNumber: string;
  citizenName: string;
  categoryName: string;
  departmentName: string;
  currentStatus: GrievanceStatus;
  createdAt: string; // ISO 8601 DateTime
  updatedAt: string; // ISO 8601 DateTime
  assignedOfficerName: string | null;
}

export interface EscalatedGrievanceDto {
  id: number;
  grievanceNumber: string;
  citizenName: string;
  categoryName: string;
  departmentName: string;
  currentStatus: GrievanceStatus;
  createdAt: string; // ISO 8601 DateTime
  updatedAt: string; // ISO 8601 DateTime
  daysSinceSubmission: number;
  daysSinceLastUpdate: number;
  assignedOfficerName: string | null;
  hasSLABreach: boolean;
}

export interface GrievanceStatusHistoryDto {
  id: number;
  grievanceId: number;
  oldStatus: GrievanceStatus;
  newStatus: GrievanceStatus;
  changedByUserId: number;
  changedByUserName: string;
  changedAt: string; // ISO 8601 DateTime
  remarks: string | null;
}

export interface LodgeGrievanceResponse {
  grievanceId: number;
  grievanceNumber: string;
  message: string;
}

export interface CanEscalateResponse {
  canEscalate: boolean;
}

// Nested DTOs
export interface AssignmentDto {
  officerId: number;
  officerName: string;
  assignedAt: string; // ISO 8601 DateTime
}

export interface ResolutionDto {
  resolvedByOfficerId: number;
  resolvedByOfficerName: string;
  resolutionRemarks: string;
  resolvedAt: string; // ISO 8601 DateTime
}

export interface FeedbackDto {
  rating: number; // 1-5
  comment: string | null;
  submittedAt: string; // ISO 8601 DateTime
}

// Officer-specific DTOs
export interface OfficerGrievanceListDto {
  id: number;
  grievanceNumber: string;
  categoryName: string;
  currentStatus: GrievanceStatus;
  description: string;
  createdAt: string; // ISO 8601 DateTime
  updatedAt: string; // ISO 8601 DateTime
  isAssignedToMe: boolean;
}

export interface OfficerGrievanceDetailDto {
  id: number;
  grievanceNumber: string;
  categoryId: number;
  categoryName: string;
  departmentId: number;
  departmentName: string;
  currentStatus: GrievanceStatus;
  description: string;
  attachmentUrl: string | null;
  createdAt: string; // ISO 8601 DateTime
  updatedAt: string; // ISO 8601 DateTime
  currentAssignment: OfficerAssignmentDto | null;
  statusHistory: OfficerStatusHistoryDto[];
}

export interface OfficerAssignmentDto {
  officerId: number;
  officerName: string;
  assignedAt: string; // ISO 8601 DateTime
  isActive: boolean;
}

export interface OfficerStatusHistoryDto {
  id: number;
  oldStatus: GrievanceStatus;
  newStatus: GrievanceStatus;
  changedByUserName: string;
  changedAt: string; // ISO 8601 DateTime
  remarks: string | null;
}

