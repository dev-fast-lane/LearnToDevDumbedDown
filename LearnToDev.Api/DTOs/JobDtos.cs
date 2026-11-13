using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

//What the API sends about a job, with the schools it requires
public record JobDto(int Id, string Name, string Description, List<SchoolDto> RequiredSchools);

//What admin sends when creating or editing a job
public class SaveJobDto
{
    [Required(ErrorMessage = "Skriv inn et navn.")]
    [StringLength(100, ErrorMessage = "Navnet kan være maks 100 tegn.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Beskrivelsen kan være maks 500 tegn.")]
    public string Description { get; set; } = string.Empty;

    //The schools the player must pass first. Empty list = open for everyone
    public List<int> RequiredSchoolIds { get; set; } = new();
}
