// Mirrors DTOs/SchoolDtos.cs and DTOs/SaveSchoolDto.cs in LearnToDev.Api

// SchoolDto: what the API sends
export interface SchoolDto {
  id: number;
  name: string;
  description: string;
}

// SaveSchoolDto: what admin sends when creating or editing
export interface SaveSchoolDto {
  name: string;
  description: string;
}