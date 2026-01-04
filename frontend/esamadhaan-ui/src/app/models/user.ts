// User model - re-export from auth
export type { UserDto, CreateUserRequest, UpdateUserRequest, UpdateUserStatusRequest } from './auth';

// Officer-specific DTOs
export interface OfficerDto {
  id: number;
  name: string;
  email: string;
  isActive: boolean;
}

export interface OfficerListDto {
  id: number;
  name: string;
  email: string;
  isActive: boolean;
}

