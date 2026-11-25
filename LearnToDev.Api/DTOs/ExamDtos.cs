using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

// An exam as the player gets it. IsCorrect is not sent, so the answers cannot be read in the browser
public record ExamDto(int SchoolId, string SchoolName, List<ExamQuestionDto> Questions);

public record ExamQuestionDto(int Id, string Text, List<ExamOptionDto> Options);

public record ExamOptionDto(int Id, string Text);

// What the player sends in: one chosen answer per question
public class SubmitExamDto
{
    [Required(ErrorMessage = "Svarene mangler.")]
    public List<ExamAnswerDto> Answers { get; set; } = new();
}

public class ExamAnswerDto
{
    public int QuestionId { get; set; }
    public int AnswerOptionId { get; set; }
}

// The result, with the correct answer and the explanation for each question
public record ExamResultDto(int Correct, int Total, bool Passed, List<ExamFeedbackDto> Feedback);

public record ExamFeedbackDto(int QuestionId, string Text, bool Correct, string CorrectAnswer, string Explanation);