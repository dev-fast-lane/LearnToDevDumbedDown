import { del, get, post, put } from "./apiClient";
import type { QuestionDto, SaveQuestionDto } from "../types/question";

// All calls to /api/questions (QuestionsController)

// Both filters are optional. Empty values are left out of the URL (R-05)
export function searchQuestions(search: string, schoolId: number | null): Promise<QuestionDto[]> {
  const params = new URLSearchParams();
  if (search.trim()) params.set("search", search.trim());
  if (schoolId !== null) params.set("schoolId", String(schoolId));
  const query = params.toString();
  return get<QuestionDto[]>(`/questions${query ? `?${query}` : ""}`);
}

export function createQuestion(dto: SaveQuestionDto): Promise<QuestionDto> {
  return post<QuestionDto>("/questions", dto);
}

export function updateQuestion(id: number, dto: SaveQuestionDto): Promise<QuestionDto> {
  return put<QuestionDto>(`/questions/${id}`, dto);
}

export function deleteQuestion(id: number): Promise<void> {
  return del<void>(`/questions/${id}`);
}
