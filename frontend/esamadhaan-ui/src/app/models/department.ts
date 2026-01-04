// Department Interfaces

// Request DTOs
export interface CreateDepartmentRequestDto {
  name: string;
  description: string;
}

export interface UpdateDepartmentRequestDto {
  name: string;
  description: string;
}

// Response DTOs
export interface DepartmentDto {
  id: number;
  name: string;
  description: string;
}

export interface CreateDepartmentResponse {
  id: number;
  name: string;
  description: string;
}

