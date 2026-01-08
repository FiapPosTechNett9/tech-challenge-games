using FIAP.CloudGames.Games.Application.Contracts.Search;
using FIAP.CloudGames.Games.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Application.Interfaces
{
    public interface IGameSearchService
    {
        Task IndexAsync(Game game, CancellationToken ct = default);
        Task RemoveAsync(Guid id, CancellationToken ct = default);

        Task<IReadOnlyList<Game>> SearchAsync(string query, int page = 1, int pageSize = 10, CancellationToken ct = default);
        Task<IReadOnlyList<Game>> GetPopularAsync(int top = 10, CancellationToken ct = default);
    }
}
