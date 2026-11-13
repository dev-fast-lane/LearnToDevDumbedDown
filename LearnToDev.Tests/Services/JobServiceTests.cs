using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using LearnToDev.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace LearnToDev.Tests.Services;

public class JobServiceTests
{
    private const string UserId = "user-1";

    private static readonly School Frontend = new() { Id = 1, Name = "Frontend" };
    private static readonly School Backend = new() { Id = 2, Name = "Backend" };

    private readonly Mock<IJobRepository> _jobRepository = new();
    private readonly Mock<IPlayerRepository> _playerRepository = new();
    private readonly Player _player = new() { Id = 7, UserId = UserId };
    private readonly JobService _service;

    public JobServiceTests()
    {
        _playerRepository.Setup(r => r.GetOrCreateAsync(UserId)).ReturnsAsync(_player);
        _service = new JobService(_jobRepository.Object, _playerRepository.Object, NullLogger<JobService>.Instance);
    }

    private void GivenJob(params School[] requiredSchools)
    {
        var job = new Job { Id = 1, Name = "Jobb" };
        foreach (var school in requiredSchools)
            job.JobRequirements.Add(new JobRequirement { JobId = 1, SchoolId = school.Id, School = school });
        _jobRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Job> { job });
    }

    private void GivenPassed(params School[] schools)
    {
        foreach (var school in schools)
            _player.PassedExams.Add(new PassedExam { PlayerId = _player.Id, SchoolId = school.Id, School = school });
    }

    [Fact]
    public async Task CheckAllJobsAsync_JobWithoutRequirements_IsOpen()
    {
        GivenJob();

        var job = Assert.Single(await _service.CheckAllJobsAsync(UserId));

        Assert.True(job.IsOpen);
        Assert.Null(job.LockReason);
    }

    [Fact]
    public async Task CheckAllJobsAsync_AllRequiredSchoolsPassed_IsOpen()
    {
        GivenPassed(Frontend, Backend);
        GivenJob(Frontend, Backend);

        var job = Assert.Single(await _service.CheckAllJobsAsync(UserId));

        Assert.True(job.IsOpen);
    }

    [Fact]
    public async Task CheckAllJobsAsync_OneSchoolMissing_IsLockedWithReason()
    {
        GivenPassed(Frontend);
        GivenJob(Frontend, Backend);

        var job = Assert.Single(await _service.CheckAllJobsAsync(UserId));

        Assert.False(job.IsOpen);
        Assert.Equal("Mangler eksamen i Backend", job.LockReason);
    }

    [Fact]
    public async Task CheckAllJobsAsync_NothingPassed_ListsAllMissingSchools()
    {
        GivenJob(Frontend, Backend);

        var job = Assert.Single(await _service.CheckAllJobsAsync(UserId));

        Assert.False(job.IsOpen);
        Assert.Equal("Mangler eksamen i Backend og Frontend", job.LockReason);
    }
}
