using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.DTOs;

// What the API sends about a school
public record SchoolDto(int Id, string Name, string Description);
