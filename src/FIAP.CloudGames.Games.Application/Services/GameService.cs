using FIAP.CloudGames.Games.Application.Contracts.Payments;
using FIAP.CloudGames.Games.Application.Contracts.Purchases;
using FIAP.CloudGames.Games.Application.Dtos;
using FIAP.CloudGames.Games.Application.Interfaces;
using FIAP.CloudGames.Games.Domain.Entities;
using FIAP.CloudGames.Games.Domain.Interfaces.Repositories;
using System.Net.Http;
using System.Net.Http.Json;

namespace FIAP.CloudGames.Games.Application.Services;

public class GameService : IGameService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IGameRepository _gameRepository;
    private readonly IGameSearchService _search;

    public GameService(IGameRepository gameRepository, IGameSearchService search, IHttpClientFactory httpClientFactory)
    {
        _gameRepository = gameRepository;
        _search = search;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Game> CreateAsync(CreateGameDto dto)
    {
        var game = new Game(
            dto.Title,
            dto.Price,
            dto.Description,
            dto.ReleaseDate,
            dto.Developer,
            dto.Publisher
        );

        await _gameRepository.AddAsync(game);

        await _search.IndexAsync(game);

        return game;
    }

    public async Task<IEnumerable<Game>> GetAllAsync()
    {
        return await _gameRepository.GetAllAsync();
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _gameRepository.GetByIdAsync(id);
    }

    public async Task<Game?> UpdateAsync(UpdateGameDto dto)
    {
        var game = await _gameRepository.GetByIdAsync(dto.Id);
        if (game == null) return null;

        game.Title = dto.Title;
        game.Price = dto.Price;
        game.Description = dto.Description;
        game.ReleaseDate = dto.ReleaseDate;
        game.Developer = dto.Developer;
        game.Publisher = dto.Publisher;

        var updated = await _gameRepository.UpdateAsync(game);

        await _search.IndexAsync(updated);

        return updated;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var game = await _gameRepository.GetByIdAsync(id);
        if (game == null) return false;

        await _gameRepository.DeleteAsync(game.Id);

        await _search.RemoveAsync(game.Id);

        return true;
    }
    public async Task<PurchaseGameResponse> PurchaseAsync(
    Guid gameId,
    Guid userId,
    CancellationToken ct = default)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game is null)
            throw new KeyNotFoundException("Game not found.");

        var orderId = Guid.NewGuid();

        var request = new CreatePaymentRequest
        {
            OrderId = orderId,
            UserId = userId,
            Amount = game.Price
        };

        var client = _httpClientFactory.CreateClient("Payments");

        var response = await client.PostAsJsonAsync(
            "payments/payments",
            request,
            ct);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException("Payment service error.");

        // Payments retorna UM GUID PURO
        var paymentId = await response.Content.ReadFromJsonAsync<Guid>(ct);

        return new PurchaseGameResponse
        {
            OrderId = orderId,
            PaymentId = paymentId,
            GameId = gameId,
            UserId = userId,
            Amount = game.Price
        };
    }
}
