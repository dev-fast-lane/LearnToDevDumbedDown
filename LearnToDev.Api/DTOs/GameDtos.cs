using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

//The player's status: current job and passed schools
public record PlayerStatusDto(string? CurrentJob, List<string> PassedSchools);

//A school as the player sees it
public record PlayerSchoolDto(int Id, string Name, string Description, bool Passed);

//An exam as the player gets it. IsCorrect is not sent, so the answers cannot be read in the browser
public record ExamDto(int SchoolId, string SchoolName, List<ExamQuestionDto> Questions);

public record ExamQuestionDto(int Id, string Text, List<ExamOptionDto> Options);

public record ExamOptionDto(int Id, string Text);

//What the player sends in: one chosen answer per question
public class SubmitExamDto
{
    [Required]
    public List<ExamAnswerDto> Answers { get; set; } = new();
}

public class ExamAnswerDto
{
    public int QuestionId { get; set; }
    public int AnswerOptionId { get; set; }
}

//The result of an exam, with the explanation for each question
public record ExamResultDto(int Correct, int Total, bool Passed, List<ExamFeedbackDto> Feedback);

public record ExamFeedbackDto(int QuestionId, string Text, bool Correct, string CorrectAnswer, string Explanation);

//A job as the player sees it: open or locked, and why
public record PlayerJobDto(int Id, string Name, string Description, List<string> RequiredSchools, bool IsOpen, string? LockReason, bool IsCurrent);
