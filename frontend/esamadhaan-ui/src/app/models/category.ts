// Category Interfaces

// Request DTOs
export interface CreateCategoryRequestDto {
  name: string;
  description: string;
  departmentId: number;
}

export interface UpdateCategoryRequestDto {
  name: string;
  description: string;
}

// Response DTOs
export interface CategoryDto {
  id: number;
  name: string;
  description: string;
  departmentId: number;
  departmentName: string;
}

export interface CreateCategoryResponse {
  id: number;
  name: string;
  description: string;
  departmentId: number;
  departmentName: string;
}

