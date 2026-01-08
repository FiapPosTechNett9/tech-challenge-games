using FIAP.CloudGames.Games.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Games.API.Controllers
{
    [ApiController]
    [Route("api/games/search")]
    public class GameSearchController : ControllerBase
    {
        private readonly IGameSearchService _search;

        public GameSearchController(IGameSearchService search)
        {
            _search = search;
        }

        /// <summary>
        /// Busca jogos por texto (title, description, developer, publisher)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Search(
            [FromQuery] string q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query 'q' é obrigatória.");

            var result = await _search.SearchAsync(q, page, pageSize, ct);
            return Ok(result);
        }

        /// <summary>
        /// Retorna jogos mais populares (por enquanto ordenados por releaseDate)
        /// </summary>
        [HttpGet("popular")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetPopular(
            [FromQuery] int top = 10,
            CancellationToken ct = default)
        {
            var result = await _search.GetPopularAsync(top, ct);
            return Ok(result);
        }
    }
}
