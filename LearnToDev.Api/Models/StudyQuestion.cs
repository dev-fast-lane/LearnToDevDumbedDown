using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.Models;

public class StudyQuestion
{
    public int Id { get; set; }

    //The school the question belongs to (SchoolId is stored in the database, School gives the object)
    public int SchoolId { get; set; }
    public School School { get; set; } = null!;

    [Required, StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    //Shown to the player after the exam
    [Required, StringLength(2000)]
    public string Explanation { get; set; } = string.Empty;

    //The answer options for the question (one question has several options, exactly one is correct)
    public ICollection<AnswerOption> AnswerOptions { get; set; } = new List<AnswerOption>();
}
