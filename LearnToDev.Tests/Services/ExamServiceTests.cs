using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LearnToDev.Tests.Services;

public class ExamServiceTests
{
    private const string UserId = "user-1";
    private const int SchoolId = 1;

    private readonly Mock<ISchoolRepository> _schoolRepository = new();
    private readonly Mock<IStudyQuestionRepository> _questionRepository = new();
    private readonly Mock<IPlayerRepository> _playerRepository = new();
    private readonly ExamService _service;

    public ExamServiceTests()
    {
        _schoolRepository.Setup(r => r.GetByIdAsync(SchoolId))
            .ReturnsAsync(new School { Id = SchoolId, Name = "Frontend" });
        _playerRepository.Setup(r => r.GetOrCreateAsync(UserId))
            .ReturnsAsync(new Player { Id = 7, UserId = UserId });

        _service = new ExamService(_schoolRepository.Object, _questionRepository.Object,
            _playerRepository.Object, NullLogger<ExamService>.Instance);
    }

    //Question n has option n*10 (correct) and n*10+1 (wrong)
    private void GivenQuestions(int count)
    {
        var questions = Enumerable.Range(1, count).Select(n => new StudyQuestion
        {
            Id = n,
            SchoolId = SchoolId,
            Text = $"Spørsmål {n}",
            Explanation = "Forklaring",
            AnswerOptions = new List<AnswerOption>
            {
                new() { Id = n * 10, Text = "Riktig", IsCorrect = true },
                new() { Id = n * 10 + 1, Text = "Feil", IsCorrect = false }
            }
        }).ToList();

        _questionRepository.Setup(r => r.GetBySchoolAsync(SchoolId)).ReturnsAsync(questions);
    }

    //Answers question 1-5, with the first `correct` answers right and the rest wrong
    private static SubmitExamDto Answers(int correct)
    {
        return new SubmitExamDto
        {
            Answers = Enumerable.Range(1, 5).Select(n => new ExamAnswerDto
            {
                QuestionId = n,
                AnswerOptionId = n <= correct ? n * 10 : n * 10 + 1
            }).ToList()
        };
    }

    [Fact]
    public async Task SubmitExamAsync_FourCorrect_PassesAndSavesPassedExam()
    {
        GivenQuestions(6);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, Answers(4));

        Assert.Equal(ServiceStatus.Success, result.Status);
        Assert.True(result.Value!.Passed);
        Assert.Equal(4, result.Value.Correct);
        _playerRepository.Verify(r => r.AddPassedExamAsync(7, SchoolId), Times.Once);
    }

    [Fact]
    public async Task SubmitExamAsync_AllCorrect_Passes()
    {
        GivenQuestions(5);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, Answers(5));

        Assert.True(result.Value!.Passed);
        Assert.Equal(5, result.Value.Correct);
    }

    [Fact]
    public async Task SubmitExamAsync_ThreeCorrect_FailsAndSavesNothing()
    {
        GivenQuestions(5);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, Answers(3));

        Assert.Equal(ServiceStatus.Success, result.Status);
        Assert.False(result.Value!.Passed);
        _playerRepository.Verify(r => r.AddPassedExamAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SubmitExamAsync_TooFewAnswers_ReturnsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers.RemoveAt(0);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitExamAsync_SameQuestionTwice_ReturnsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers[4].QuestionId = 1;

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitExamAsync_QuestionFromOtherSchool_ReturnsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers[0].QuestionId = 99;

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
        _playerRepository.Verify(r => r.AddPassedExamAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SubmitExamAsync_UnknownSchool_ReturnsNotFound()
    {
        var result = await _service.SubmitExamAsync(UserId, 42, Answers(5));

        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task StartExamAsync_EnoughQuestions_ReturnsFiveDifferentQuestions()
    {
        GivenQuestions(8);

        var result = await _service.StartExamAsync(SchoolId);

        Assert.Equal(ServiceStatus.Success, result.Status);
        Assert.Equal(GameRules.ExamQuestionCount, result.Value!.Questions.Count);
        Assert.Equal(GameRules.ExamQuestionCount, result.Value.Questions.Select(q => q.Id).Distinct().Count());
    }

    [Fact]
    public async Task StartExamAsync_TooFewQuestions_ReturnsConflict()
    {
        GivenQuestions(4);

        var result = await _service.StartExamAsync(SchoolId);

        Assert.Equal(ServiceStatus.Conflict, result.Status);
    }
}
