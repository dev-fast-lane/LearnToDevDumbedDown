// Mirrors the question DTOs in LearnToDev.Api. API sends camelCase

// What the API sends
export interface AnswerOptionDto {
  id: number;
  text: string;
  isCorrect: boolean;
}

export interface QuestionDto {
  id: number;
  schoolId: number;
  schoolName: string;
  text: string;
  explanation: string;
  answerOptions: AnswerOptionDto[];
}

// What the API takes in. No ids: the question id is in the URL, and the options get new ids
export interface SaveAnswerOptionDto {
  text: string;
  isCorrect: boolean;
}

export interface SaveQuestionDto {
  schoolId: number;
  text: string;
  explanation: string;
  answerOptions: SaveAnswerOptionDto[];
}
