using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using FIAP.CloudGames.Games.Application.Contracts.Search;
using FIAP.CloudGames.Games.Application.Interfaces;
using FIAP.CloudGames.Games.Domain.Entities;
using FIAP.CloudGames.Games.Infrastructure.Configuration.Search;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FIAP.CloudGames.Games.Infrastructure.Search
{
    public sealed class GameSearchService : IGameSearchService
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticsearchSettings _settings;

        public GameSearchService(ElasticsearchClient client, ElasticsearchSettings settings)
        {
            _client = client;
            _settings = settings;
        }

        public async Task IndexAsync(Game game, CancellationToken ct = default)
        {
            var doc = GameSearchDocument.FromEntity(game);

            var resp = await _client.IndexAsync(doc, i => i
                .Index(_settings.Index)
                .Id(doc.Id), ct);

            if (!resp.IsValidResponse)
                throw new InvalidOperationException($"Elasticsearch index failed: {resp.DebugInformation}");
        }

        public async Task RemoveAsync(Guid id, CancellationToken ct = default)
        {
            var resp = await _client.DeleteAsync<GameSearchDocument>(id, d => d
                .Index(_settings.Index), ct);

            // Se não existir, ok.
            if (!resp.IsValidResponse && resp.ApiCallDetails?.HttpStatusCode is not 404)
                throw new InvalidOperationException($"Elasticsearch delete failed: {resp.DebugInformation}");
        }

        public async Task<IReadOnlyList<Game>> SearchAsync(string query, int page = 1, int pageSize = 10, CancellationToken ct = default)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize is < 1 or > 100 ? 10 : pageSize;

            var from = (page - 1) * pageSize;

            var resp = await _client.SearchAsync<GameSearchDocument>(s => s
                .Index(_settings.Index)
                .From(from)
                .Size(pageSize)
                .Query(q => q.Bool(b => b
                    .Should(
                        sh => sh.MultiMatch(mm => mm
                            .Query(query)
                            .Fields(new[] { "title^3", "description", "developer", "publisher" })
                            .Type(TextQueryType.BestFields)
                            .Fuzziness("AUTO")
                        ),
                        sh => sh.Match(m => m.Field(f => f.Title).Query(query))
                    )
                    .MinimumShouldMatch(1)
                )), ct);

            if (!resp.IsValidResponse)
                throw new InvalidOperationException($"Elasticsearch search failed: {resp.DebugInformation}");

            // Convertendo de volta para entidade (mínimo)
            return resp.Documents.Select(d => new Game(
                d.Title, d.Price, d.Description, d.ReleaseDate, d.Developer, d.Publisher
            )
            {
                Id = d.Id
            }).ToList();
        }

        public async Task<IReadOnlyList<Game>> GetPopularAsync(int top = 10, CancellationToken ct = default)
        {
            top = top is < 1 or > 50 ? 10 : top;

            var resp = await _client.SearchAsync<GameSearchDocument>(s => s
                .Index(_settings.Index)
                .Size(top)
                .Sort(so => so.Field(f => f
                    .Field(ff => ff.ReleaseDate)
                    .Order(SortOrder.Desc)
                )), ct);

            if (!resp.IsValidResponse)
                throw new InvalidOperationException($"Elasticsearch popular failed: {resp.DebugInformation}");

            return resp.Documents.Select(d => new Game(
                d.Title, d.Price, d.Description, d.ReleaseDate, d.Developer, d.Publisher
            )
            {
                Id = d.Id
            }).ToList();
        }
    }
}