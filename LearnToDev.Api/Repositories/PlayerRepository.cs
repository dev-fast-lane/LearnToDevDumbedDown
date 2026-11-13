using LearnToDev.Api.Data;
using LearnToDev.Api.Models;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LearnToDev.Api.Repositories;

public class PlayerRepository : IPlayerRepository
{
    private readonly ApplicationDbContext _context;

    public PlayerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Player> GetOrCreateAsync(string userId)
    {
        var player = await _context.Players
            .Include(p => p.CurrentJob)
            .Include(p => p.PassedExams)
                .ThenInclude(e => e.School)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (player != null)
            return player;

        player = new Player { UserId = userId };
        _context.Players.Add(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task SetCurrentJobAsync(int playerId, int? jobId)
    {
        var player = await _context.Players.FindAsync(playerId)
                     ?? throw new InvalidOperationException($"Player {playerId} does not exist.");

        player.CurrentJobId = jobId;
        await _context.SaveChangesAsync();
    }

    public async Task AddPassedExamAsync(int playerId, int schoolId)
    {
        if (await _context.PassedExams.FindAsync(playerId, schoolId) != null)
            return;

        _context.PassedExams.Add(new PassedExam
        {
            PlayerId = playerId,
            SchoolId = schoolId,
            PassedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
}
