// Mirrors DTOs/QuestionDtos.cs in LearnToDev.Api

// AnswerOptionDto
export interface AnswerOptionDto {
  id: number;
  text: string;
  isCorrect: boolean;
}

// QuestionDto
export interface QuestionDto {
  id: number;
  schoolId: number;
  schoolName: string;
  text: string;
  explanation: string;
  answerOptions: AnswerOptionDto[];
}

// SaveAnswerOptionDto
export interface SaveAnswerOptionDto {
  text: string;
  isCorrect: boolean;
}

// SaveQuestionDto
export interface SaveQuestionDto {
  schoolId: number;
  text: string;
  explanation: string;
  answerOptions: SaveAnswerOptionDto[];
}
