using LearnToDev.Api.Data;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Api.Repositories;

public class SchoolRepository : ISchoolRepository
{
    private readonly ApplicationDbContext _context;

    public SchoolRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<School>> GetAllAsync()
    {
        return await _context.Schools.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<School?> GetByIdAsync(int id)
    {
        return await _context.Schools.FindAsync(id);
    }

    //The database sets Id when we save, so the returned object has the new Id
    public async Task<School> AddAsync(School school)
    {
        _context.Schools.Add(school);
        await _context.SaveChangesAsync();
        return school;
    }

    //Returns false if the school does not exist, so the controller can answer 404
    public async Task<bool> UpdateAsync(School school)
    {
        var existing = await _context.Schools.FindAsync(school.Id);
        if (existing == null)
            return false;

        existing.Name = school.Name;
        existing.Description = school.Description;
        await _context.SaveChangesAsync();
        return true;
    }

    //Deleting a school also deletes its questions, job requirements and passed exams (cascade)
    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _context.Schools.FindAsync(id);
        if (existing == null)
            return false;

        _context.Schools.Remove(existing);
        await _context.SaveChangesAsync();
        return true;
    }
}
