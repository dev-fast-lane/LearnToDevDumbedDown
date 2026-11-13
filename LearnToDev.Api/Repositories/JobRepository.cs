using LearnToDev.Api.Data;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Api.Repositories;

public class JobRepository : IJobRepository
{
    private readonly ApplicationDbContext _context;

    public JobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Job>> GetAllAsync()
    {
        return await WithRequirements().OrderBy(j => j.JobRequirements.Count).ThenBy(j => j.Name).ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(int id)
    {
        return await WithRequirements().FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<Job> AddAsync(Job job)
    {
        _context.Jobs.Add(job);
        await _context.SaveChangesAsync();
        return job;
    }

    //Replaces the name, the description and the required schools
    public async Task<bool> UpdateAsync(Job job)
    {
        var existing = await _context.Jobs
            .Include(j => j.JobRequirements)
            .FirstOrDefaultAsync(j => j.Id == job.Id);
        if (existing == null)
            return false;

        existing.Name = job.Name;
        existing.Description = job.Description;

        _context.JobRequirements.RemoveRange(existing.JobRequirements);
        existing.JobRequirements = job.JobRequirements;

        await _context.SaveChangesAsync();
        return true;
    }

    //Players with this job become unemployed (SetNull in ApplicationDbContext)
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Jobs.FindAsync(id);
        if (existing == null)
            return false;

        _context.Jobs.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }

    private IQueryable<Job> WithRequirements()
    {
        return _context.Jobs
            .Include(j => j.JobRequirements)
                .ThenInclude(r => r.School);
    }
}
