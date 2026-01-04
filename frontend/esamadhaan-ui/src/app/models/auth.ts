// Authentication & User Interfaces

// Request DTOs
export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
  role: string; // "Citizen" | "DepartmentOfficer" | "SupervisoryOfficer" | "SystemAdmin"
  departmentId?: number | null;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface CreateUserRequest {
  name: string;
  email: string;
  password: string;
  role: string;
  departmentId?: number | null;
}

export interface UpdateUserRequest {
  name: string;
  email: string;
  role?: string | null;
  departmentId?: number | null;
}

export interface UpdateUserStatusRequest {
  isActive: boolean;
}

// Response DTOs
export interface LoginResponse {
  userId: number;
  name: string;
  email: string;
  role: string;
  departmentId: number | null;
  token: string;
}

export interface ProfileResponse {
  id: number;
  name: string;
  email: string;
  role: string;
  departmentId: number | null;
  departmentName: string | null;
  isActive: boolean;
  createdAt: string; // ISO 8601 DateTime
}

export interface EmailCheckResponse {
  email: string;
  isAvailable: boolean;
}

export interface StatusUpdateResponse {
  success: boolean;
  message: string;
}

// UserDto is same as ProfileResponse
export type UserDto = ProfileResponse;

