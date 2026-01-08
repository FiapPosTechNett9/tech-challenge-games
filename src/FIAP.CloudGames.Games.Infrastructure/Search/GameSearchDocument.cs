using FIAP.CloudGames.Games.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Infrastructure.Search
{
    public sealed class GameSearchDocument
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public decimal Price { get; init; }
        public DateTime ReleaseDate { get; init; }
        public string? Developer { get; init; }
        public string? Publisher { get; init; }

        public static GameSearchDocument FromEntity(Game game) => new()
        {
            Id = game.Id,
            Title = game.Title,
            Description = game.Description,
            Price = game.Price,
            ReleaseDate = game.ReleaseDate,
            Developer = game.Developer,
            Publisher = game.Publisher
        };
    }
}
