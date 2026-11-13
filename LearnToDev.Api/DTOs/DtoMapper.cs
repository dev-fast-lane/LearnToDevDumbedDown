using LearnToDev.Api.Models;

namespace LearnToDev.Api.DTOs;

//Turns models into DTOs and back, so the controllers never send or receive the entities (R-09)
public static class DtoMapper
{
    public static SchoolDto ToDto(this School school) =>
        new(school.Id, school.Name, school.Description);

    public static School ToModel(this SaveSchoolDto dto, int id = 0) =>
        new() { Id = id, Name = dto.Name.Trim(), Description = dto.Description.Trim() };

    public static QuestionDto ToDto(this StudyQuestion question) =>
        new(question.Id, question.SchoolId, question.School?.Name ?? string.Empty, question.Text, question.Explanation,
            question.AnswerOptions.OrderBy(o => o.Id).Select(o => new AnswerOptionDto(o.Id, o.Text, o.IsCorrect)).ToList());

    public static StudyQuestion ToModel(this SaveQuestionDto dto, int id = 0) =>
        new()
        {
            Id = id,
            SchoolId = dto.SchoolId,
            Text = dto.Text.Trim(),
            Explanation = dto.Explanation.Trim(),
            AnswerOptions = dto.AnswerOptions
                .Select(o => new AnswerOption { Text = o.Text.Trim(), IsCorrect = o.IsCorrect })
                .ToList()
        };

    public static JobDto ToDto(this Job job) =>
        new(job.Id, job.Name, job.Description,
            job.JobRequirements.Select(r => r.School.ToDto()).OrderBy(s => s.Name).ToList());

    public static Job ToModel(this SaveJobDto dto, int id = 0) =>
        new()
        {
            Id = id,
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            JobRequirements = dto.RequiredSchoolIds
                .Distinct()
                .Select(schoolId => new JobRequirement { JobId = id, SchoolId = schoolId })
                .ToList()
        };
}
