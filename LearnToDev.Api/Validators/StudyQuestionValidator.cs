using System.ComponentModel.DataAnnotations;
using LearnToDev.Api.DTOs;

namespace LearnToDev.Api.Validators;

// The rules that need several fields at once, so they can't be attributes on a single property.
// SaveQuestionDto.Validate calls this, so [ApiController] runs it automatically, and it can be tested on its own
public static class StudyQuestionValidator
{
    public static IEnumerable<ValidationResult> Validate(SaveQuestionDto dto)
    {
        // != 1 covers both "no correct answer" and "several correct answers"
        if (dto.AnswerOptions.Count(o => o.IsCorrect) != 1)
        {
            yield return new ValidationResult(
                "Nøyaktig ett svar må være riktig.",
                new[] { nameof(SaveQuestionDto.AnswerOptions) });
        }
    }
}
