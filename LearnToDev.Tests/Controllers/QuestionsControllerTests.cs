using LearnToDev.Api.Controllers;
using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LearnToDev.Tests.Controllers;

public class QuestionsControllerTests
{
    private readonly Mock<IStudyQuestionRepository> _questions = new();
    private readonly Mock<ISchoolRepository> _schools = new();
    private readonly QuestionsController _controller;

    public QuestionsControllerTests()
    {
        _controller = new QuestionsController(_questions.Object, _schools.Object,
            NullLogger<QuestionsController>.Instance);
    }

    private static SaveQuestionDto ValidDto(int schoolId) => new()
    {
        SchoolId = schoolId,
        Text = "Spørsmål",
        Explanation = "Forklaring",
        AnswerOptions = new List<SaveAnswerDto>
        {
            new() { Text = "a", IsCorrect = true },
            new() { Text = "b", IsCorrect = false }
        }
    };

    [Fact]
    public async Task GetById_QuestionDoesNotExist_ReturnsNotFound()
    {
        _questions.Setup(r => r.GetByIdAsync(5)).ReturnsAsync((StudyQuestion?)null);

        var result = await _controller.GetById(5);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_SchoolDoesNotExist_ReturnsValidationErrorAndSavesNothing()
    {
        _schools.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((School?)null);

        var result = await _controller.Create(ValidDto(999));

        // ValidationProblem gives an ObjectResult with ValidationProblemDetails (400 when the app runs)
        var objectResult = Assert.IsAssignableFrom<ObjectResult>(result.Result);
        Assert.IsType<ValidationProblemDetails>(objectResult.Value);
        Assert.True(_controller.ModelState.ContainsKey(nameof(SaveQuestionDto.SchoolId)));
        _questions.Verify(r => r.AddAsync(It.IsAny<StudyQuestion>()), Times.Never);
    }

    [Fact]
    public async Task Update_QuestionDoesNotExist_ReturnsNotFound()
    {
        _schools.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new School { Id = 1, Name = "Backend" });
        _questions.Setup(r => r.UpdateAsync(It.IsAny<StudyQuestion>())).ReturnsAsync(false);

        var result = await _controller.Update(5, ValidDto(1));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_QuestionExists_ReturnsNoContent()
    {
        _questions.Setup(r => r.DeleteAsync(5)).ReturnsAsync(true);

        var result = await _controller.Delete(5);

        Assert.IsType<NoContentResult>(result);
    }
}
