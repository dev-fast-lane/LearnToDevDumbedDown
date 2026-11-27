import { get, post } from "./apiClient";
import type { ExamDto, ExamResultDto, PlayerSchoolDto, PlayerStatusDto, SubmitExamDto, PlayerJobDto } from "../types/game";

// Alle kall til /api/game. Komponentene bruker denne, ikke fetch
export function getStatus(): Promise<PlayerStatusDto> {
    return get<PlayerStatusDto>("/game/status");
}

// Trekker en ny eksamen fra skolen (GET /api/game/exams/{schoolId})
export function startExam(schoolId: number): Promise<ExamDto> {
    return get<ExamDto>(`/game/exams/${schoolId}`);
}

// Leverer svarene og får resultatet tilbake (POST /api/game/exams/{schoolId})
export function submitExam(schoolId: number, dto: SubmitExamDto): Promise<ExamResultDto> {
    return post<ExamResultDto>(`/game/exams/${schoolId}`, dto);
}

// Skoleoversikten: alle skolene med om spilleren har bestått (GET /api/game/schools)
export function getGameSchools(): Promise<PlayerSchoolDto[]> {
    return get<PlayerSchoolDto[]>("/game/schools");
}

// Jobbsiden: alle jobbene, åpne eller låste (GET /api/game/jobs)
export function getPlayerJobs(): Promise<PlayerJobDto[]> {
    return get<PlayerJobDto[]>("/game/jobs");
}

// Tar en jobb. En låst jobb gir ApiError med årsaken (POST /api/game/jobs/{jobId}/take)
export function takeJob(jobId: number): Promise<PlayerJobDto> {
    return post<PlayerJobDto>(`/game/jobs/${jobId}/take`);
}

// Sier opp jobben. API-et svarer 204 uten innhold (POST /api/game/jobs/quit)
export function quitJob(): Promise<void> {
    return post<void>("/game/jobs/quit");
}