// Mirrors DTOs/SchoolDtos.cs in LearnToDev.Api. The API sends camelCase

// SchoolDto
export interface SchoolDto {
  id: number;
  name: string;
  description: string;
}

// SaveSchoolDto
export interface SaveSchoolDto {
  name: string;
  description: string;
}
