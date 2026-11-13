using LearnToDev.Api.Models;

namespace LearnToDev.Api.Repositories.Interfaces;

public interface IJobRepository
{
    //Jobs come with their required schools
    Task<List<Job>> GetAllAsync();
    Task<Job?> GetByIdAsync(int id);
    Task<Job> AddAsync(Job job);
    Task<bool> UpdateAsync(Job job);
    Task<bool> DeleteAsync(int id);
}
