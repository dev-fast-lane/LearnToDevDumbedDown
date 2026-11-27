using LearnToDev.Api.Models;

namespace LearnToDev.Api.DTOs;

public static class QuestionMapper
{
    //Model -> DTO (what the API sends). Requires School to be loaded with Include, otherwise School is null
    public static QuestionDto ToDto(this StudyQuestion question)
    {
        return new QuestionDto(
            question.Id,
            question.SchoolId,
            question.School.Name,
            question.Text,
            question.Explanation,
            question.AnswerOptions
                .Select(o => new AnswerOptionDto(o.Id, o.Text, o.IsCorrect))
                .ToList());
    }

    //DTO -> model (what the API saves). No Id: it comes from the URL, and the answer options get new ids from the database
    public static StudyQuestion ToModel(this SaveQuestionDto dto)
    {
        return new StudyQuestion
        {
            SchoolId = dto.SchoolId,
            Text = dto.Text,
            Explanation = dto.Explanation,
            AnswerOptions = dto.AnswerOptions
                .Select(o => new AnswerOption { Text = o.Text, IsCorrect = o.IsCorrect })
                .ToList()
        };
    }
}
