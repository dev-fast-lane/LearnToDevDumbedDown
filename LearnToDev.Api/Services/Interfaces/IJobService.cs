namespace LearnToDev.Api.Services.Interfaces;

// Decides which jobs a player can take, and why the others are locked
public interface IJobService
{
    // Every job, marked open or locked for the player with this user id
    Task<List<JobEligibility>> CheckAllJobsAsync(string userId);

    // Only allowed when the player has passed every school the job requires
    Task<ServiceResult<JobEligibility>> TakeJobAsync(string userId, int jobId);

    // Only allowed when the player has a job
    Task<ServiceResult<bool>> QuitJobAsync(string userId);
}
