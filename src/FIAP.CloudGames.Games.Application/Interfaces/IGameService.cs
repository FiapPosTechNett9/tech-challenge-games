using FIAP.CloudGames.Games.Application.Dtos;
using FIAP.CloudGames.Games.Domain.Entities;

namespace FIAP.CloudGames.Games.Application.Interfaces;

public interface IGameService
{
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(Guid id);
    Task<Game> CreateAsync(CreateGameDto dto);
    Task<Game?> UpdateAsync(UpdateGameDto dto);
    Task<bool> DeleteAsync(Guid id);
}
