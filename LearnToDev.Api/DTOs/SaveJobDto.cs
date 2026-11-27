using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

// Det admin sender når en jobb lages eller endres. Id-en kommer fra URL-en eller databasen
public class SaveJobDto
{
    [Required(ErrorMessage = "Skriv inn et navn.")]
    [StringLength(100, ErrorMessage = "Navnet kan være maks 100 tegn.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Beskrivelsen kan være maks 500 tegn.")]
    public string Description { get; set; } = string.Empty;

    // Id-ene til skolene jobben krever. En tom liste betyr at jobben er åpen for alle
    public List<int> RequiredSchoolIds { get; set; } = new();
}