using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

//What the API sends to admin about a question, including which answer is correct
public record QuestionDto(int Id, int SchoolId, string SchoolName, string Text, string Explanation, List<AnswerOptionDto> AnswerOptions);

public record AnswerOptionDto(int Id, string Text, bool IsCorrect);

//What admin sends when creating or editing a question
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

    [Required]
    [MinLength(2, ErrorMessage = "Et spørsmål må ha minst 2 svaralternativer.")]
    [MaxLength(4, ErrorMessage = "Et spørsmål kan ha maks 4 svaralternativer.")]
    public List<SaveAnswerOptionDto> AnswerOptions { get; set; } = new();

    //Rules that need more than one field. [ApiController] runs this and answers 400 if it fails
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (AnswerOptions.Count(o => o.IsCorrect) != 1)
        {
            yield return new ValidationResult("Nøyaktig ett svaralternativ må være riktig.", new[] { nameof(AnswerOptions) });
        }
    }
}

public class SaveAnswerOptionDto
{
    [Required(ErrorMessage = "Skriv inn svaralternativet.")]
    [StringLength(1000, ErrorMessage = "Svaralternativet kan være maks 1000 tegn.")]
    public string Text { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
