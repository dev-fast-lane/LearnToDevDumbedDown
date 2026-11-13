using LearnToDev.Api.DTOs;

namespace LearnToDev.Api.Services.Interfaces;

public interface IExamService
{
    //Draws GameRules.ExamQuestionCount random questions from the school
    Task<ServiceResult<ExamDto>> StartExamAsync(int schoolId);

    //Grades the answers, and saves the passed exam if the player got GameRules.ExamPassMark or more correct
    Task<ServiceResult<ExamResultDto>> SubmitExamAsync(string userId, int schoolId, SubmitExamDto dto);
}
