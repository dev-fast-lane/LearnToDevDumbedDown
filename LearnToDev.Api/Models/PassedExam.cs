namespace LearnToDev.Api.Models;

//Join table for the N:M relation between Player and School: the player has passed the exam in the school
public class PassedExam
{
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public int SchoolId { get; set; }
    public School School { get; set; } = null!;

    public DateTime PassedAt { get; set; }
}
