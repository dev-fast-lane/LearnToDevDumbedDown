using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

//What the API sends about a school
public record SchoolDto(int Id, string Name, string Description);

//What admin sends when creating or editing a school. No Id: it comes from the URL or the database
public class SaveSchoolDto
{
    [Required(ErrorMessage = "Skriv inn et navn.")]
    [StringLength(100, ErrorMessage = "Navnet kan være maks 100 tegn.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Beskrivelsen kan være maks 500 tegn.")]
    public string Description { get; set; } = string.Empty;
}
