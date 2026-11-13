import type { SchoolDto } from "./school";

// Mirrors DTOs/JobDtos.cs in LearnToDev.Api

// JobDto
export interface JobDto {
  id: number;
  name: string;
  description: string;
  requiredSchools: SchoolDto[];
}

// SaveJobDto
export interface SaveJobDto {
  name: string;
  description: string;
  requiredSchoolIds: number[];
}
