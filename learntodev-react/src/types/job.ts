import type { SchoolDto } from "./school";

// Samme felt som DTOs/JobDto.cs og DTOs/SaveJobDto.cs i API-et

// Det API-et sender om en jobb, med skolene jobben krever
export interface JobDto {
  id: number;
  name: string;
  description: string;
  requiredSchools: SchoolDto[];
}

// Det admin sender når en jobb lages eller endres
export interface SaveJobDto {
  name: string;
  description: string;
  requiredSchoolIds: number[];
}