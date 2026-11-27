using System.ComponentModel.DataAnnotations;
using LearnToDev.Api.Validators;

namespace LearnToDev.Api.DTOs;

public record AnswerOptionDto(int Id, string Text, bool IsCorrect);

public record QuestionDto(int Id, int SchoolId, string SchoolName, string Text, string Explanation, List<AnswerOptionDto> AnswerOptions);

public class SaveQuestionDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Velg en skole.")]
    public int SchoolId { get; set; }

    [Required(ErrorMessage = "Skriv inn spørsmålet.")]
    [StringLength(1000, ErrorMessage = "Spørsmålet kan være maks 1000 tegn.")]
    public string Text { get; set; } = string.Empty;

    [Required(ErrorMessage = "Skriv inn en forklaring.")]
    [StringLength(2000, ErrorMessage = "Forklaringen kan være maks 2000 tegn.")]
    public string Explanation { get; set; } = string.Empty;

    [Required(ErrorMessage = "Legg til svaralternativer.")]
    [MinLength(2, ErrorMessage = "Spørsmålet må ha minst 2 svaralternativer.")]
    [MaxLength(4, ErrorMessage = "Spørsmålet kan ha maks 4 svaralternativer.")]
    public List<SaveAnswerDto> AnswerOptions { get; set; } = new();

    // Runs after the attributes pass. The rule itself is in StudyQuestionValidator
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return StudyQuestionValidator.Validate(this);
    }

}

public class SaveAnswerDto
{
    [Required(ErrorMessage = "Skriv inn svaralternativet.")]
    [StringLength(1000, ErrorMessage = "Svaralternativet kan være maks 1000 tegn.")]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}