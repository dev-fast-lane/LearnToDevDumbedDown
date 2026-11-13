import { get, post } from "./apiClient";
import type {
  ExamDto,
  ExamResultDto,
  PlayerJobDto,
  PlayerSchoolDto,
  PlayerStatusDto,
  SubmitExamDto,
} from "../types/game";

// All calls to /api/game (GameController). The API knows the player from the login cookie

export function getStatus(): Promise<PlayerStatusDto> {
  return get<PlayerStatusDto>("/game/status");
}

export function getPlayerSchools(): Promise<PlayerSchoolDto[]> {
  return get<PlayerSchoolDto[]>("/game/schools");
}

export function startExam(schoolId: number): Promise<ExamDto> {
  return get<ExamDto>(`/game/exams/${schoolId}`);
}

export function submitExam(schoolId: number, dto: SubmitExamDto): Promise<ExamResultDto> {
  return post<ExamResultDto>(`/game/exams/${schoolId}`, dto);
}

export function getPlayerJobs(): Promise<PlayerJobDto[]> {
  return get<PlayerJobDto[]>("/game/jobs");
}

export function takeJob(jobId: number): Promise<PlayerJobDto> {
  return post<PlayerJobDto>(`/game/jobs/${jobId}/take`);
}

export function quitJob(): Promise<void> {
  return post<void>("/game/jobs/quit");
}
