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
    private readonly Player _player = new() { Id = 7, UserId = UserId };
    private readonly ExamService _service;

    public ExamServiceTests()
    {
        //Bare skole 1 finnes. Andre id-er gir null, fordi Moq returnerer default når ingenting er satt opp
        _schoolRepository.Setup(r => r.GetByIdAsync(SchoolId))
            .ReturnsAsync(new School { Id = SchoolId, Name = "Frontend" });
        _playerRepository.Setup(r => r.GetOrCreateAsync(UserId)).ReturnsAsync(_player);

        _service = new ExamService(_schoolRepository.Object, _questionRepository.Object,
            _playerRepository.Object, NullLogger<ExamService>.Instance);
    }

    //Et spørsmål med ett riktig svar (id*10+1) og ett feil svar (id*10+2), så id-ene er lette å regne ut
    private static StudyQuestion Question(int id) => new()
    {
        Id = id,
        SchoolId = SchoolId,
        Text = $"Spørsmål {id}",
        Explanation = $"Forklaring {id}",
        AnswerOptions = new List<AnswerOption>
        {
            new() { Id = id * 10 + 1, StudyQuestionId = id, Text = "Riktig", IsCorrect = true },
            new() { Id = id * 10 + 2, StudyQuestionId = id, Text = "Feil" }
        }
    };

    //Skolen får spørsmålene 1, 2, ..., count
    private void GivenQuestions(int count)
    {
        var questions = Enumerable.Range(1, count).Select(Question).ToList();
        _questionRepository.Setup(r => r.GetBySchoolAsync(SchoolId)).ReturnsAsync(questions);
    }

    //Svar på spørsmål 1-5, der de første correctCount er riktige
    private static SubmitExamDto Answers(int correctCount) => new()
    {
        Answers = Enumerable.Range(1, GameRules.ExamQuestionCount)
            .Select(id => new ExamAnswerDto
            {
                QuestionId = id,
                AnswerOptionId = id <= correctCount ? id * 10 + 1 : id * 10 + 2
            })
            .ToList()
    };

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    public async Task SubmitExamAsync_FourOrMoreCorrect_PassesAndSaves(int correct)
    {
        GivenQuestions(5);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, Answers(correct));

        Assert.Equal(ServiceStatus.Success, result.Status);
        Assert.True(result.Value!.Passed);
        Assert.Equal(correct, result.Value.Correct);
        _playerRepository.Verify(r => r.AddPassedExamAsync(_player.Id, SchoolId), Times.Once);
    }

    [Fact]
    public async Task SubmitExamAsync_ThreeCorrect_FailsAndDoesNotSave()
    {
        GivenQuestions(5);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, Answers(3));

        Assert.Equal(ServiceStatus.Success, result.Status);
        Assert.False(result.Value!.Passed);
        _playerRepository.Verify(r => r.AddPassedExamAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task SubmitExamAsync_TooFewAnswers_IsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers.RemoveAt(0);

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitExamAsync_SameQuestionTwice_IsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers[4].QuestionId = 1; //5 svar, men bare 4 ulike spørsmål

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitExamAsync_QuestionFromAnotherSchool_IsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers[4] = new ExamAnswerDto { QuestionId = 99, AnswerOptionId = 991 }; //Finnes ikke i skole 1

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task SubmitExamAsync_OptionFromAnotherQuestion_IsInvalid()
    {
        GivenQuestions(5);
        var dto = Answers(5);
        dto.Answers[0].AnswerOptionId = 21; //Svaret hører til spørsmål 2, ikke 1

        var result = await _service.SubmitExamAsync(UserId, SchoolId, dto);

        Assert.Equal(ServiceStatus.Invalid, result.Status);
    }

    [Fact]
    public async Task StartExamAsync_UnknownSchool_IsNotFound()
    {
        var result = await _service.StartExamAsync(99);

        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task StartExamAsync_EnoughQuestions_DrawsFiveDifferent()
    {
        GivenQuestions(8);

        var result = await _service.StartExamAsync(SchoolId);

        Assert.Equal(ServiceStatus.Success, result.Status);
        var questions = result.Value!.Questions;
        Assert.Equal(GameRules.ExamQuestionCount, questions.Count);
        Assert.Equal(questions.Count, questions.Select(q => q.Id).Distinct().Count());
        Assert.All(questions, q => Assert.Equal(2, q.Options.Count));
    }

    [Fact]
    public async Task StartExamAsync_TooFewQuestions_IsConflict()
    {
        GivenQuestions(4);

        var result = await _service.StartExamAsync(SchoolId);

        Assert.Equal(ServiceStatus.Conflict, result.Status);
    }

    //Skolene GetAllAsync gir. Rekkefølgen er bevisst ikke sortert, så testen viser at servicen sorterer
    private void GivenSchools(params School[] schools)
    {
        _schoolRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(schools.ToList());
    }

    [Fact]
    public async Task GetSchoolsAsync_PassedOne_OnlyThatSchoolIsPassedAndSortedByName()
    {
        GivenSchools(
            new School { Id = 1, Name = "Frontend" },
            new School { Id = 2, Name = "Backend" });
        _player.PassedExams.Add(new PassedExam { PlayerId = _player.Id, SchoolId = 1 });

        var schools = await _service.GetSchoolsAsync(UserId);

        Assert.Equal(new[] { "Backend", "Frontend" }, schools.Select(s => s.Name));
        Assert.True(schools.Single(s => s.Id == 1).Passed);
        Assert.False(schools.Single(s => s.Id == 2).Passed);
    }

    [Fact]
    public async Task GetSchoolsAsync_NewPlayer_NoSchoolIsPassed()
    {
        GivenSchools(
            new School { Id = 1, Name = "Frontend" },
            new School { Id = 2, Name = "Backend" });

        var schools = await _service.GetSchoolsAsync(UserId);

        Assert.Equal(2, schools.Count);
        Assert.All(schools, s => Assert.False(s.Passed));
    }
}