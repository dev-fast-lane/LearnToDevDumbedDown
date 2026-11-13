using System.ComponentModel.DataAnnotations;
using LearnToDev.Api.DTOs;

namespace LearnToDev.Tests.DTOs;

//Runs the same validation as [ApiController] does before the controller is called
public class SaveQuestionDtoTests
{
    private static SaveQuestionDto NewDto(params bool[] correct)
    {
        return new SaveQuestionDto
        {
            SchoolId = 1,
            Text = "Hva står HTML for?",
            Explanation = "HyperText Markup Language.",
            AnswerOptions = correct.Select((isCorrect, i) => new SaveAnswerOptionDto
            {
                Text = $"Svar {i}",
                IsCorrect = isCorrect
            }).ToList()
        };
    }

    private static List<ValidationResult> Validate(SaveQuestionDto dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void Validate_ExactlyOneCorrect_IsValid()
    {
        var errors = Validate(NewDto(true, false, false));

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_NoCorrectAnswer_IsInvalid()
    {
        var errors = Validate(NewDto(false, false, false));

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void Validate_TwoCorrectAnswers_IsInvalid()
    {
        var errors = Validate(NewDto(true, true, false));

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SaveQuestionDto.AnswerOptions)));
    }

    [Fact]
    public void Validate_OnlyOneOption_IsInvalid()
    {
        var errors = Validate(NewDto(true));

        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SaveQuestionDto.AnswerOptions)));
    }
}
