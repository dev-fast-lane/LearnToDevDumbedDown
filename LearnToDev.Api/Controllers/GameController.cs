using System.Security.Claims;
using LearnToDev.Api.DTOs;
using LearnToDev.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearnToDev.Api.Controllers;

//Spillet for den innloggede brukeren. Alle endepunktene krever innlogging
[ApiController]
[Route("api/game")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly ILogger<GameController> _logger;

    public GameController(IPlayerRepository playerRepository, ILogger<GameController> logger)
    {
        _playerRepository = playerRepository;
        _logger = logger;
    }
    
    //Startskjermen: nåværende jobb og beståtte eksamener
    [HttpGet("status")]
    public async Task<ActionResult<PlayerStatusDto>> GetStatus()
    {
        //Id-en kommer fra innloggingscookien, ikke fra URL-en, så ingen kan hente andres spill
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        //Første gang brukeren åpner spillet, lages spilleren her
        var player = await _playerRepository.GetOrCreateAsync(userId);
        _logger.LogDebug("Spiller {PlayerId} hentet status.", player.Id);

        //Sender bare det startskjermen trenger. Skolene sorteres, så rekkefølgen alltid er den samme
        return Ok(new PlayerStatusDto
        {
            CurrentJobName = player.CurrentJob?.Name,
            PassedSchools = player.PassedExams
                .Select(e => e.School.Name)
                .OrderBy(name => name)
                .ToList()
        });
    }
}