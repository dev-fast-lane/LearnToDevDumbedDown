import { del, get, post, put } from "./apiClient";
import type { QuestionDto, SaveQuestionDto } from "../types/question";

// All calls to /api/questions (admin only). Components use this, not fetch
export function getQuestions(): Promise<QuestionDto[]> {
  return get<QuestionDto[]>("/questions");
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
