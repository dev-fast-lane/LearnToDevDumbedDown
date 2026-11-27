namespace LearnToDev.Api.DTOs;

// Det API-et sender om en jobb, med skolene jobben krever
public record JobDto(int Id, string Name, string Description, List<SchoolDto> RequiredSchools);