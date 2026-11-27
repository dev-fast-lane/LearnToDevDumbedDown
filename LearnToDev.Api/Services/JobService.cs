using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services.Interfaces;

namespace LearnToDev.Api.Services;

// The job rule (R-03): a job is open when the player has passed every school it requires
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

    public async Task<List<JobEligibility>> CheckAllJobsAsync(string userId)
    {
        var player = await _playerRepository.GetOrCreateAsync(userId);
        var jobs = await _jobRepository.GetAllAsync();

        var results = jobs.Select(job => ToEligibility(job, player)).ToList();

        _logger.LogDebug("Player {PlayerId} has {OpenCount} of {JobCount} jobs open",
            player.Id, results.Count(r => r.IsOpen), results.Count);
        return results;
    }

    public async Task<ServiceResult<JobEligibility>> TakeJobAsync(string userId, int jobId)
    {
        var job = await _jobRepository.GetByIdAsync(jobId);
        if (job == null)
            return ServiceResult<JobEligibility>.NotFound("Fant ikke jobben.");

        var player = await _playerRepository.GetOrCreateAsync(userId);

        // The same rule as on the jobs page, checked again on the server, so a locked job cannot be taken
        var lockReason = GetLockReason(job, player);
        if (lockReason != null)
        {
            _logger.LogWarning("Player {PlayerId} tried to take locked job {JobId}", player.Id, jobId);
            return ServiceResult<JobEligibility>.Conflict(lockReason);
        }

        await _playerRepository.SetCurrentJobAsync(player.Id, job.Id);
        player.CurrentJobId = job.Id; // so IsCurrent is true in the answer
        _logger.LogInformation("Player {PlayerId} took job {JobId}", player.Id, jobId);

        return ServiceResult<JobEligibility>.Ok(ToEligibility(job, player));
    }

    public async Task<ServiceResult<bool>> QuitJobAsync(string userId)
    {
        var player = await _playerRepository.GetOrCreateAsync(userId);
        if (player.CurrentJobId == null)
            return ServiceResult<bool>.Conflict("Du har ingen jobb å si opp.");

        _logger.LogInformation("Player {PlayerId} quit job {JobId}", player.Id, player.CurrentJobId);
        await _playerRepository.SetCurrentJobAsync(player.Id, null);

        return ServiceResult<bool>.Ok(true);
    }

    // Builds the answer for one job and one player
    private static JobEligibility ToEligibility(Job job, Player player)
    {
        var lockReason = GetLockReason(job, player);
        return new JobEligibility(
            job.Id,
            job.Name,
            job.Description,
            job.JobRequirements.Select(r => r.School.Name).OrderBy(name => name).ToList(),
            lockReason == null,
            lockReason,
            player.CurrentJobId == job.Id);
    }

    // null when the job is open, otherwise e.g. "Mangler eksamen i Backend og Frontend"
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

    // Joins a list the Norwegian way: "A", "A og B", "A, B og C"
    private static string JoinWithAnd(List<string> items)
    {
        if (items.Count == 1)
            return items[0];

        return $"{string.Join(", ", items.Take(items.Count - 1))} og {items[^1]}";
    }
}
