// Mirrors DTOs/GameDtos.cs in LearnToDev.Api

// PlayerStatusDto
export interface PlayerStatusDto {
  currentJob: string | null;
  passedSchools: string[];
}

// PlayerSchoolDto
export interface PlayerSchoolDto {
  id: number;
  name: string;
  description: string;
  passed: boolean;
}

// ExamDto. The answers are not sent, so they cannot be read in the browser
export interface ExamDto {
  schoolId: number;
  schoolName: string;
  questions: {
    id: number;
    text: string;
    options: { id: number; text: string }[];
  }[];
}

// SubmitExamDto
export interface SubmitExamDto {
  answers: { questionId: number; answerOptionId: number }[];
}

// ExamResultDto
export interface ExamResultDto {
  correct: number;
  total: number;
  passed: boolean;
  feedback: {
    questionId: number;
    text: string;
    correct: boolean;
    correctAnswer: string;
    explanation: string;
  }[];
}

// PlayerJobDto
export interface PlayerJobDto {
  id: number;
  name: string;
  description: string;
  requiredSchools: string[];
  isOpen: boolean;
  lockReason: string | null;
  isCurrent: boolean;
}
