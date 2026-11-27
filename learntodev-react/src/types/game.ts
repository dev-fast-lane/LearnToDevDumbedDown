// Samme felt som PlayerStatusDto i API-et. API-et sender camelCase
export interface PlayerStatusDto {
    currentJobName: string | null;
    passedSchools: string[];
}
// Samme felt som DTO-ene i ExamDtos.cs. Svaralternativene har ikke isCorrect, så fasiten ikke ligger i nettleseren
export interface ExamDto {
    schoolId: number;
    schoolName: string;
    questions: ExamQuestionDto[];
}

export interface ExamQuestionDto {
    id: number;
    text: string;
    options: ExamOptionDto[];
}

export interface ExamOptionDto {
    id: number;
    text: string;
}

// Det som sendes inn: ett valgt svar per spørsmål
export interface SubmitExamDto {
    answers: ExamAnswerDto[];
}

export interface ExamAnswerDto {
    questionId: number;
    answerOptionId: number;
}

// Resultatet, med riktig svar og forklaring for hvert spørsmål
export interface ExamResultDto {
    correct: number;
    total: number;
    passed: boolean;
    feedback: ExamFeedbackDto[];
}

export interface ExamFeedbackDto {
    questionId: number;
    text: string;
    correct: boolean;
    correctAnswer: string;
    explanation: string;
}

export interface PlayerSchoolDto {
    id: number;
    name: string;
    description: string;
    passed: boolean;
}