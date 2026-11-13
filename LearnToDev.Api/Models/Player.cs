using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace LearnToDev.Api.Models;

//The game state of one user. Created the first time the user opens the game
public class Player
{
    public int Id { get; set; }

    //Link to the Identity login user. One user has one player
    [Required]
    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    //Current job. null means the player is unemployed
    public int? CurrentJobId { get; set; }
    public Job? CurrentJob { get; set; }

    //The schools the player has passed (one player has many)
    public ICollection<PassedExam> PassedExams { get; set; } = new List<PassedExam>();

    //NB: PassedExams must be loaded first (Include). There is no lazy loading
    public bool HasPassed(int schoolId)
    {
        return PassedExams.Any(p => p.SchoolId == schoolId);
    }
}
