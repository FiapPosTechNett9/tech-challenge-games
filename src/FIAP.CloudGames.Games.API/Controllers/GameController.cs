using FIAP.CloudGames.Games.Application.Dtos;
using FIAP.CloudGames.Games.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FIAP.CloudGames.Games.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private static readonly ActivitySource ActivitySource =
        new("games-service");

    private readonly IGameService _gameService;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<GameController> _logger;

    public GameController(
        IGameService gameService,
        IHttpClientFactory httpClientFactory,
        ILogger<GameController> logger)
    {
        _gameService = gameService;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetAll()
    {
        using var activity = ActivitySource.StartActivity(
            "GetAllGames",
            ActivityKind.Internal);

        _logger.LogInformation("Buscando todos os games");

        var games = await _gameService.GetAllAsync();
        return Ok(games);
    }

    [HttpGet("{id}")]
    //  [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> GetById(Guid id)
    {
        using var activity = ActivitySource.StartActivity(
            "GetGameById",
            ActivityKind.Internal);

        activity?.SetTag("game.id", id);

        _logger.LogInformation("GetById game solicitado {GameId}", id);

        // 🔗 CHAMADA AO USERS SERVICE (distributed tracing)
        var httpClient = _httpClientFactory.CreateClient();

        // Aqui o objetivo é só demonstrar comunicação entre serviços
        // Exemplo: validar se o usuário autenticado existe
        var userIdHeader = HttpContext.User?.FindFirst("sub")?.Value;

        var usersServiceUrl = $"http://localhost:5117/api/User/{id}";

        var userResponse = await httpClient.GetAsync(usersServiceUrl);

        var game = await _gameService.GetByIdAsync(id);

        if (game is null)
        {
            activity?.SetTag("game.found", false);
            _logger.LogWarning("Game não encontrado {GameId}", id);
            return NotFound();
        }

        activity?.SetTag("game.found", true);

        return Ok(game);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateGameDto dto)
    {
        using var activity = ActivitySource.StartActivity(
            "CreateGame",
            ActivityKind.Internal);

        _logger.LogInformation("Create game solicitado {@GameDto}", dto);

        var createdGame = await _gameService.CreateAsync(dto);

        activity?.SetTag("game.id", createdGame.Id);

        _logger.LogInformation("Game criado com sucesso {GameId}", createdGame.Id);

        return CreatedAtAction(nameof(GetById), new { id = createdGame.Id }, createdGame);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateGameDto dto)
    {
        using var activity = ActivitySource.StartActivity(
            "UpdateGame",
            ActivityKind.Internal);

        activity?.SetTag("game.id", dto.Id);

        _logger.LogInformation("Update game solicitado {GameId}", dto.Id);

        var updated = await _gameService.UpdateAsync(dto);

        if (updated is null)
        {
            _logger.LogWarning("Update falhou, game não encontrado {GameId}", dto.Id);
            return NotFound();
        }

        _logger.LogInformation("Game atualizado com sucesso {GameId}", dto.Id);

        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        using var activity = ActivitySource.StartActivity(
            "DeleteGame",
            ActivityKind.Internal);

        activity?.SetTag("game.id", id);

        _logger.LogInformation("Delete game solicitado {GameId}", id);

        var deleted = await _gameService.DeleteAsync(id);

        if (!deleted)
        {
            _logger.LogWarning("Delete falhou, game não encontrado {GameId}", id);
            return NotFound();
        }

        _logger.LogInformation("Game deletado com sucesso {GameId}", id);

        return NoContent();
    }
}
