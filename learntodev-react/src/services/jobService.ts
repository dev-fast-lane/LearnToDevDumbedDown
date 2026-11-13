import { del, get, post, put } from "./apiClient";
import type { JobDto, SaveJobDto } from "../types/job";

// All calls to /api/jobs (JobsController, admin only)

export function getJobs(): Promise<JobDto[]> {
  return get<JobDto[]>("/jobs");
}

export function createJob(dto: SaveJobDto): Promise<JobDto> {
  return post<JobDto>("/jobs", dto);
}

export function updateJob(id: number, dto: SaveJobDto): Promise<JobDto> {
  return put<JobDto>(`/jobs/${id}`, dto);
}

export function deleteJob(id: number): Promise<void> {
  return del<void>(`/jobs/${id}`);
}
