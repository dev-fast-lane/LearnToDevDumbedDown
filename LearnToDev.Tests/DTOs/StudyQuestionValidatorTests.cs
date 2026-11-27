using System.ComponentModel.DataAnnotations;
using LearnToDev.Api.DTOs;
using LearnToDev.Api.Validators;

namespace LearnToDev.Tests.DTOs;

public class StudyQuestionValidatorTests
{
    // A valid question. Each test changes only the one thing it checks
    private static SaveQuestionDto ValidQuestion() => new()
    {
        SchoolId = 1,
        Text = "Hvilken datatype brukes for heltall?",
        Explanation = "int brukes for heltall.",
        AnswerOptions = new List<SaveAnswerDto>
        {
            new() { Text = "int", IsCorrect = true },
            new() { Text = "string", IsCorrect = false }
        }
    };

    // Runs both the attributes and IValidatableObject.Validate, like [ApiController] does.
    // Validate only runs when the attributes pass
    private static List<ValidationResult> ValidateAll(SaveQuestionDto dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }

    private static bool HasErrorOn(IEnumerable<ValidationResult> results, string member) =>
        results.Any(r => r.MemberNames.Contains(member));

    [Fact]
    public void Validate_OneCorrectAnswer_ReturnsNoErrors()
    {
        var results = StudyQuestionValidator.Validate(ValidQuestion());

        Assert.Empty(results);
    }

    [Fact]
    public void Validate_NoCorrectAnswer_ReturnsErrorOnAnswerOptions()
    {
        var dto = ValidQuestion();
        dto.AnswerOptions[0].IsCorrect = false;

        var results = StudyQuestionValidator.Validate(dto);

        Assert.True(HasErrorOn(results, nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void Validate_TwoCorrectAnswers_ReturnsErrorOnAnswerOptions()
    {
        var dto = ValidQuestion();
        dto.AnswerOptions[1].IsCorrect = true;

        var results = StudyQuestionValidator.Validate(dto);

        Assert.True(HasErrorOn(results, nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void TryValidateObject_ValidQuestion_ReturnsNoErrors()
    {
        Assert.Empty(ValidateAll(ValidQuestion()));
    }

    [Fact]
    public void TryValidateObject_TwoCorrectAnswers_RunsValidatorAndReturnsError()
    {
        var dto = ValidQuestion();
        dto.AnswerOptions[1].IsCorrect = true;

        Assert.True(HasErrorOn(ValidateAll(dto), nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void TryValidateObject_OnlyOneOption_ReturnsError()
    {
        var dto = ValidQuestion();
        dto.AnswerOptions.RemoveAt(1);

        Assert.True(HasErrorOn(ValidateAll(dto), nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void TryValidateObject_FiveOptions_ReturnsError()
    {
        var dto = ValidQuestion();
        for (var i = 0; i < 3; i++)
            dto.AnswerOptions.Add(new SaveAnswerDto { Text = $"Ekstra {i}" });

        Assert.True(HasErrorOn(ValidateAll(dto), nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void TryValidateObject_EmptyText_ReturnsErrorOnText()
    {
        var dto = ValidQuestion();
        dto.Text = "";

        Assert.True(HasErrorOn(ValidateAll(dto), nameof(SaveQuestionDto.Text)));
    }

    [Fact]
    public void TryValidateObject_NoSchool_ReturnsErrorOnSchoolId()
    {
        var dto = ValidQuestion();
        dto.SchoolId = 0;

        Assert.True(HasErrorOn(ValidateAll(dto), nameof(SaveQuestionDto.SchoolId)));
    }
}
