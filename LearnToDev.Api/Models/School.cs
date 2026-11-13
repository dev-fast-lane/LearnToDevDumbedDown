using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.Models;

public class School
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    //The questions in the school. The exam draws from these
    public ICollection<StudyQuestion> StudyQuestions { get; set; } = new List<StudyQuestion>();

    //Jobs that require this school (N:M through JobRequirement)
    public ICollection<JobRequirement> JobRequirements { get; set; } = new List<JobRequirement>();

    //Players who have passed the exam (N:M through PassedExam)
    public ICollection<PassedExam> PassedExams { get; set; } = new List<PassedExam>();
}
