using LearnToDev.Api.DTOs;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services.Interfaces;

namespace LearnToDev.Api.Services;

//The job rules (R-03): a job is open when the player has passed every school it requires
public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<JobService> _logger;

    public JobService(IJobRepository jobRepository, IPlayerRepository playerRepository, ILogger<JobService> logger)
    {
        _jobRepository = jobRepository;
        _playerRepository = playerRepository;
        _logger = logger;
    }

    public async Task<List<PlayerJobDto>> GetJobsForPlayerAsync(string userId)
    {
        var player = await _playerRepository.GetOrCreateAsync(userId);
        var jobs = await _jobRepository.GetAllAsync();

        return jobs.Select(job => ToPlayerJobDto(job, player)).ToList();
    }

    public async Task<ServiceResult<PlayerJobDto>> TakeJobAsync(string userId, int jobId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
            return ServiceResult<PlayerJobDto>.NotFound("Fant ikke jobben.");

        var player = await _playerRepository.GetOrCreateAsync(userId);

        var lockReason = GetLockReason(job, player);
        if (lockReason != null)
        {
            _logger.LogWarning("User {UserId} tried to take locked job {JobId}", userId, jobId);
            return ServiceResult<PlayerJobDto>.Conflict(lockReason);
        }

        await _playerRepository.SetCurrentJobAsync(player.Id, job.Id);
        player.CurrentJobId = job.Id;
        _logger.LogInformation("User {UserId} took job {JobId}", userId, jobId);

        return ServiceResult<PlayerJobDto>.Ok(ToPlayerJobDto(job, player));
    }

    public async Task<ServiceResult<bool>> QuitJobAsync(string userId)
    {
        var player = await _playerRepository.GetOrCreateAsync(userId);
        if (player.CurrentJobId == null)
            return ServiceResult<bool>.Conflict("Du har ingen jobb å si opp.");

        await _playerRepository.SetCurrentJobAsync(player.Id, null);
        _logger.LogInformation("User {UserId} quit job {JobId}", userId, player.CurrentJobId);

        return ServiceResult<bool>.Ok(true);
    }

    //null when the job is open, otherwise e.g. "Mangler eksamen i Frontend og Backend"
    private static string? GetLockReason(Job job, Player player)
    {
        var missing = job.JobRequirements
            .Where(r => !player.HasPassed(r.SchoolId))
            .Select(r => r.School.Name)
            .OrderBy(name => name)
            .ToList();

        if (missing.Count == 0)
            return null;

        return $"Mangler eksamen i {JoinWithAnd(missing)}";
    }

    private static PlayerJobDto ToPlayerJobDto(Job job, Player player)
    {
        var lockReason = GetLockReason(job, player);
        return new PlayerJobDto(
            job.Id,
            job.Name,
            job.Description,
            job.JobRequirements.Select(r => r.School.Name).OrderBy(n => n).ToList(),
            lockReason == null,
            lockReason,
            player.CurrentJobId == job.Id);
    }

    //Joins a list the Norwegian way: "A", "A og B", "A, B og C"
    private static string JoinWithAnd(List<string> items)
    {
        if (items.Count == 1)
            return items[0];

        return $"{string.Join(", ", items.Take(items.Count - 1))} og {items[^1]}";
    }
}
