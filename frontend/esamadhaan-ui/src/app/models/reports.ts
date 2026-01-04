import { GrievanceStatus } from './common';

// Reports & Analytics Interfaces

export interface DashboardSummaryDto {
  totalGrievances: number;
  submittedGrievances: number;
  assignedGrievances: number;
  inReviewGrievances: number;
  resolvedGrievances: number;
  closedGrievances: number;
  resolutionRate: number;
  averageResolutionTimeInDays: number;
  averageFeedbackRating: number;
  recentGrievances: RecentGrievanceDto[];
  recentAssignments: RecentAssignmentDto[];
  topCategories: Record<string, number>;
  myAssignedGrievances?: number;
  myResolvedGrievances?: number;
}

export interface RecentGrievanceDto {
  id: number;
  grievanceNumber: string;
  status: string;
  createdAt: string; // ISO 8601 DateTime
}

export interface RecentAssignmentDto {
  grievanceId: number;
  grievanceNumber: string;
  officerName: string;
  assignedAt: string; // ISO 8601 DateTime
}

export interface GrievanceStatusReportDto {
  status: GrievanceStatus;
  count: number;
  percentage: number;
}

export interface ResolutionTimeReportDto {
  averageResolutionTimeInDays: number;
  medianResolutionTimeInDays: number;
  minResolutionTimeInDays: number;
  maxResolutionTimeInDays: number;
  totalResolvedGrievances: number;
  resolutionTimeByDepartment: Record<string, number>;
  resolutionTimeByCategory: Record<string, number>;
}

export interface DepartmentPerformanceReportDto {
  departmentId: number;
  departmentName: string;
  totalGrievances: number;
  resolvedGrievances: number;
  pendingGrievances: number;
  resolutionRate: number;
  averageResolutionTimeInDays: number;
  averageFeedbackRating: number;
  activeOfficerCount: number;
}

export interface OfficerPerformanceReportDto {
  officerId: number;
  officerName: string;
  departmentName: string;
  totalAssignedGrievances: number;
  resolvedGrievances: number;
  pendingGrievances: number;
  resolutionRate: number;
  averageResolutionTimeInDays: number;
  averageFeedbackRating: number;
}

export interface FeedbackAnalyticsReportDto {
  averageRating: number;
  totalFeedbackCount: number;
  feedbackCountByRating: Record<number, number>; // Key: 1-5, Value: count
  ratingPercentages: Record<number, number>; // Key: 1-5, Value: percentage
  positiveFeedbackCount: number; // Rating >= 4
  negativeFeedbackCount: number; // Rating <= 2
  positiveFeedbackPercentage: number;
}

