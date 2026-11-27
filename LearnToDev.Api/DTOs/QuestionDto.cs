using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

public record AnswerOptionDto(int Id, string Text, bool IsCorrect);

public record QuestionDto(int Id, int SchoolId, string SchoolName, string Text, string Explanation, List<AnswerOptionDto> AnswerOptions);

public class SaveQuestionDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Choose a school!")]
    public int SchoolId { get; set; }

    [Required(ErrorMessage = "Write the question!")]
    [StringLength(1000, ErrorMessage = "The max question length is 1000 characters.")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Writhe the explanation")]
    [StringLength(2000, ErrorMessage = "Explanation shouldn't be over 2000 characters long.")]
    public string Explanation { get; set; } = string.Empty;

    [Required(ErrorMessage = "OFc it needs Answer options!!!")]
    [MinLength(2, ErrorMessage = "Must have at least 2 options")]
    [MaxLength(4, ErrorMessage = "At most 4 options")]
    public List<SaveAnswerDto> AnswerOptions { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AnswerOptions.Count(o => o.IsCorrect) != 1)
        {
            yield return new ValidationResult(
                "Needs exactly 1 correct option",
                new[] { nameof(AnswerOptions) });
        }
    }

}

public class SaveAnswerDto
{
    [Required(ErrorMessage = "Write the correct answer.")]
    [StringLength(1000, ErrorMessage = "The answer should not exceed 1000 characters.")]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}