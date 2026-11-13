namespace LearnToDev.Api.Services.Interfaces;

// Decides which jobs a player can take, and why the others are locked
public interface IJobService
{
    // Every job, marked open or locked for the player with this user id
    Task<List<JobEligibility>> CheckAllJobsAsync(string userId);
}
