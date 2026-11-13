using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services.Interfaces;

namespace LearnToDev.Api.Services;

//The exam rules (R-03): 5 random questions, 4 correct to pass. Grading happens here on the server
public class ExamService : IExamService
{
    private readonly ISchoolRepository _schoolRepository;
    private readonly IStudyQuestionRepository _questionRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<ExamService> _logger;

    public ExamService(ISchoolRepository schoolRepository, IStudyQuestionRepository questionRepository,
        IPlayerRepository playerRepository, ILogger<ExamService> logger)
    {
        _schoolRepository = schoolRepository;
        _questionRepository = questionRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<ExamDto>> StartExamAsync(int schoolId)
    {
        var school = await _schoolRepository.GetByIdAsync(schoolId);
        if (school == null)
            return ServiceResult<ExamDto>.NotFound("Fant ikke skolen.");

        var questions = await _questionRepository.GetBySchoolAsync(schoolId);
        if (questions.Count < GameRules.ExamQuestionCount)
        {
            _logger.LogWarning("School {SchoolId} has only {Count} questions, too few for an exam", schoolId, questions.Count);
            return ServiceResult<ExamDto>.Conflict(
                $"{school.Name} har for få spørsmål til en eksamen (trenger {GameRules.ExamQuestionCount}).");
        }

        //Random order, then take the first 5. The answer options are shuffled too
        var drawn = questions
            .OrderBy(_ => Random.Shared.Next())
            .Take(GameRules.ExamQuestionCount)
            .Select(q => new ExamQuestionDto(q.Id, q.Text, q.AnswerOptions
                .OrderBy(_ => Random.Shared.Next())
                .Select(o => new ExamOptionDto(o.Id, o.Text))
                .ToList()))
            .ToList();

        return ServiceResult<ExamDto>.Ok(new ExamDto(school.Id, school.Name, drawn));
    }

    public async Task<ServiceResult<ExamResultDto>> SubmitExamAsync(string userId, int schoolId, SubmitExamDto dto)
    {
        var school = await _schoolRepository.GetByIdAsync(schoolId);
        if (school == null)
            return ServiceResult<ExamResultDto>.NotFound("Fant ikke skolen.");

        //Exactly one answer for each of 5 different questions
        var answeredIds = dto.Answers.Select(a => a.QuestionId).Distinct().Count();
        if (dto.Answers.Count != GameRules.ExamQuestionCount || answeredIds != GameRules.ExamQuestionCount)
            return ServiceResult<ExamResultDto>.Invalid(
                $"Du må svare på nøyaktig {GameRules.ExamQuestionCount} ulike spørsmål.");

        var questions = (await _questionRepository.GetBySchoolAsync(schoolId)).ToDictionary(q => q.Id);

        var feedback = new List<ExamFeedbackDto>();
        foreach (var answer in dto.Answers)
        {
            //The question must belong to this school, and the option to the question
            if (!questions.TryGetValue(answer.QuestionId, out var question))
                return ServiceResult<ExamResultDto>.Invalid("Et av spørsmålene hører ikke til denne skolen.");

            if (question.AnswerOptions.All(o => o.Id != answer.AnswerOptionId))
                return ServiceResult<ExamResultDto>.Invalid("Et av svarene hører ikke til spørsmålet.");

            var correctOption = question.AnswerOptions.Single(o => o.IsCorrect);
            feedback.Add(new ExamFeedbackDto(question.Id, question.Text,
                correctOption.Id == answer.AnswerOptionId, correctOption.Text, question.Explanation));
        }

        var correct = feedback.Count(f => f.Correct);
        var passed = correct >= GameRules.ExamPassMark;

        if (passed)
        {
            var player = await _playerRepository.GetOrCreateAsync(userId);
            await _playerRepository.AddPassedExamAsync(player.Id, schoolId);
        }

        _logger.LogInformation("User {UserId} got {Correct} of {Total} on the exam in school {SchoolId}. Passed: {Passed}",
            userId, correct, feedback.Count, schoolId, passed);

        return ServiceResult<ExamResultDto>.Ok(new ExamResultDto(correct, feedback.Count, passed, feedback));
    }
}
