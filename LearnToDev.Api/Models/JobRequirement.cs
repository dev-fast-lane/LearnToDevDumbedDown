namespace LearnToDev.Api.Models;

//Join table for the N:M relation between Job and School: the job requires the exam in the school
public class JobRequirement
{
    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public int SchoolId { get; set; }
    public School School { get; set; } = null!;
}
