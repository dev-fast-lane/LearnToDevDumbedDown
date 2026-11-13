import { del, get, post, put } from "./apiClient";
import type { SaveSchoolDto, SchoolDto } from "../types/school";

// All calls to /api/schools (SchoolsController). Components use this, not fetch

export function getSchools(): Promise<SchoolDto[]> {
  return get<SchoolDto[]>("/schools");
}

export function createSchool(dto: SaveSchoolDto): Promise<SchoolDto> {
  return post<SchoolDto>("/schools", dto);
}

export function updateSchool(id: number, dto: SaveSchoolDto): Promise<SchoolDto> {
  return put<SchoolDto>(`/schools/${id}`, dto);
}

export function deleteSchool(id: number): Promise<void> {
  return del<void>(`/schools/${id}`);
}
