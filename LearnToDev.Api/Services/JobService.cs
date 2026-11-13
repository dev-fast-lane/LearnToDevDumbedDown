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

        var results = jobs.Select(job =>
        {
            var lockReason = GetLockReason(job, player);
            return new JobEligibility(job.Id, job.Name, lockReason == null, lockReason);
        }).ToList();

        _logger.LogDebug("Player {PlayerId} has {OpenCount} of {JobCount} jobs open",
            player.Id, results.Count(r => r.IsOpen), results.Count);
        return results;
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
