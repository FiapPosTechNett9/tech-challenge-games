using FIAP.CloudGames.Games.Domain.Entities;

namespace FIAP.CloudGames.Games.Domain.Interfaces.Repositories;

public interface IGameRepository
{
    Task AddAsync(Game game);
    Task<IEnumerable<Game>> GetAllAsync();
    Task<Game?> GetByIdAsync(Guid id);
    Task<Game?> UpdateAsync(Game game);
    Task DeleteAsync(Guid id);
}
