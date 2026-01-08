using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Application.Contracts.Search
{
    public sealed class GameSearchResultDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = default!;
        public string? Description { get; init; }
        public string? Genre { get; init; }
        public decimal Price { get; init; }
        public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
        public long PurchaseCount { get; init; }
    }
}
