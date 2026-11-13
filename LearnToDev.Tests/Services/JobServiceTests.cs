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

    private static Job NewJob(int id, params School[] requiredSchools)
    {
        var job = new Job { Id = id, Name = $"Jobb {id}" };
        foreach (var school in requiredSchools)
            job.JobRequirements.Add(new JobRequirement { JobId = id, SchoolId = school.Id, School = school });
        return job;
    }

    private void GivenPassed(params School[] schools)
    {
        foreach (var school in schools)
            _player.PassedExams.Add(new PassedExam { PlayerId = _player.Id, SchoolId = school.Id, School = school });
    }

    [Fact]
    public async Task GetJobsForPlayerAsync_JobWithoutRequirements_IsOpen()
    {
        _jobRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Job> { NewJob(1) });

        var jobs = await _service.GetJobsForPlayerAsync(UserId);

        Assert.True(Assert.Single(jobs).IsOpen);
    }

    [Fact]
    public async Task GetJobsForPlayerAsync_AllRequiredSchoolsPassed_IsOpen()
    {
        GivenPassed(Frontend, Backend);
        _jobRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Job> { NewJob(1, Frontend, Backend) });

        var jobs = await _service.GetJobsForPlayerAsync(UserId);

        Assert.True(Assert.Single(jobs).IsOpen);
    }

    [Fact]
    public async Task GetJobsForPlayerAsync_OneSchoolMissing_IsLockedWithReason()
    {
        GivenPassed(Frontend);
        _jobRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Job> { NewJob(1, Frontend, Backend) });

        var job = Assert.Single(await _service.GetJobsForPlayerAsync(UserId));

        Assert.False(job.IsOpen);
        Assert.Equal("Mangler eksamen i Backend", job.LockReason);
    }

    [Fact]
    public async Task TakeJobAsync_OpenJob_SetsCurrentJob()
    {
        GivenPassed(Frontend);
        _jobRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(NewJob(1, Frontend));

        var result = await _service.TakeJobAsync(UserId, 1);

        Assert.Equal(ServiceStatus.Success, result.Status);
        Assert.True(result.Value!.IsCurrent);
        _playerRepository.Verify(r => r.SetCurrentJobAsync(7, 1), Times.Once);
    }

    [Fact]
    public async Task TakeJobAsync_LockedJob_ReturnsConflictAndKeepsJob()
    {
        _jobRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(NewJob(1, Frontend, Backend));

        var result = await _service.TakeJobAsync(UserId, 1);

        Assert.Equal(ServiceStatus.Conflict, result.Status);
        Assert.Equal("Mangler eksamen i Backend og Frontend", result.Message);
        _playerRepository.Verify(r => r.SetCurrentJobAsync(It.IsAny<int>(), It.IsAny<int?>()), Times.Never);
    }

    [Fact]
    public async Task TakeJobAsync_UnknownJob_ReturnsNotFound()
    {
        var result = await _service.TakeJobAsync(UserId, 99);

        Assert.Equal(ServiceStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task QuitJobAsync_HasJob_RemovesJob()
    {
        _player.CurrentJobId = 1;

        var result = await _service.QuitJobAsync(UserId);

        Assert.Equal(ServiceStatus.Success, result.Status);
        _playerRepository.Verify(r => r.SetCurrentJobAsync(7, null), Times.Once);
    }

    [Fact]
    public async Task QuitJobAsync_NoJob_ReturnsConflict()
    {
        var result = await _service.QuitJobAsync(UserId);

        Assert.Equal(ServiceStatus.Conflict, result.Status);
    }
}
