using System.ComponentModel.DataAnnotations;

namespace LearnToDev.Api.Models;

public class Job
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    //The schools the player must have passed to take the job (N:M through JobRequirement)
    public ICollection<JobRequirement> JobRequirements { get; set; } = new List<JobRequirement>();

    //Players who have this job now
    public ICollection<Player> Players { get; set; } = new List<Player>();
}
