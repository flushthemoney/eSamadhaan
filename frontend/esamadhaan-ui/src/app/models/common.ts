// Common Enums and Types

export enum GrievanceStatus {
  Submitted = 1,
  Assigned = 2,
  InReview = 3,
  Resolved = 4,
  Closed = 5,
}

export const GrievanceStatusLabels: Record<GrievanceStatus, string> = {
  [GrievanceStatus.Submitted]: "Submitted",
  [GrievanceStatus.Assigned]: "Assigned",
  [GrievanceStatus.InReview]: "In Review",
  [GrievanceStatus.Resolved]: "Resolved",
  [GrievanceStatus.Closed]: "Closed",
};

