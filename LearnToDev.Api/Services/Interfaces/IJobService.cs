using LearnToDev.Api.DTOs;

namespace LearnToDev.Api.Services.Interfaces;

//Decides which jobs a player can take, and lets the player take or quit a job
public interface IJobService
{
    //Every job, marked open or locked for this player
    Task<List<PlayerJobDto>> GetJobsForPlayerAsync(string userId);

    //Only allowed when the player has passed every school the job requires
    Task<ServiceResult<PlayerJobDto>> TakeJobAsync(string userId, int jobId);

    Task<ServiceResult<bool>> QuitJobAsync(string userId);
}
