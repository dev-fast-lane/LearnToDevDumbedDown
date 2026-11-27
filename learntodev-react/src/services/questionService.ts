import { del, get, post, put } from "./apiClient";
import type { QuestionDto, SaveQuestionDto } from "../types/question";

// All calls to /api/questions (admin only). Components use this, not fetch

// Both filters are optional (#25). URLSearchParams adds & and encodes spaces, # and æøå
export function getQuestions(search?: string, schoolId?: number): Promise<QuestionDto[]> {
  const params = new URLSearchParams();
  if (search && search.trim() !== "") params.set("search", search.trim());
  if (schoolId !== undefined) params.set("schoolId", String(schoolId));

  const query = params.toString(); // e.g. "search=html&schoolId=1", or "" without filters
  return get<QuestionDto[]>(query ? `/questions?${query}` : "/questions");
}

export function getQuestion(id: number): Promise<QuestionDto> {
  return get<QuestionDto>(`/questions/${id}`);
}

// 201 with the created question
export function createQuestion(dto: SaveQuestionDto): Promise<QuestionDto> {
  return post<QuestionDto>("/questions", dto);
}

// 204, so nothing comes back
export function updateQuestion(id: number, dto: SaveQuestionDto): Promise<void> {
  return put<void>(`/questions/${id}`, dto);
}

export function deleteQuestion(id: number): Promise<void> {
  return del<void>(`/questions/${id}`);
}
