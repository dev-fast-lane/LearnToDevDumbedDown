using LearnToDev.Api.Models;

namespace LearnToDev.Api.Repositories.Interfaces;

public interface IPlayerRepository
{
    //The player for a user, with current job and passed exams. Created the first time
    Task<Player> GetOrCreateAsync(string userId);
    Task SetCurrentJobAsync(int playerId, int? jobId);
    //Does nothing if the player has already passed the school
    Task AddPassedExamAsync(int playerId, int schoolId);
}
