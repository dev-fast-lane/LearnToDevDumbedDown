import { get, post } from "./apiClient";
import type { ExamDto, ExamResultDto, PlayerSchoolDto, PlayerStatusDto, SubmitExamDto } from "../types/game";

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