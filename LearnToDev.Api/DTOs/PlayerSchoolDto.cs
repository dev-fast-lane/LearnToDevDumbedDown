namespace LearnToDev.Api.DTOs;

// En skole slik spilleren ser den i skoleoversikten, med om eksamen er bestått
public record PlayerSchoolDto(int Id, string Name, string Description, bool Passed);