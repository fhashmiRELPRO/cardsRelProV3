using Microsoft.AspNetCore.Mvc;
using RelPro.Contracts.Common;
using RelPro.CARDS.Api.Search.Models;
using RelPro.CARDS.Api.Search.Repositories;

namespace RelPro.CARDS.Api.Search.Controllers;

[ApiController]
[Route("v1/search")]
[Produces("application/json")]
public sealed class SearchController : ControllerBase
{
    private readonly IPersonSearchRepository _repo;

    public SearchController(IPersonSearchRepository repo) => _repo = repo;

    /// <summary>
    /// Full-text people search against the MySQL `individuals` table.
    /// </summary>
    /// <remarks>
    /// Searches name, email, title, and company fields. Results are ordered by relevance.
    /// `limit` is clamped to [1, 100] - values outside this range default to 20.
    ///
    /// **Error codes:**
    /// - `MISSING_QUERY` - `q` parameter is required and must not be blank
    /// - `MISSING_TOKEN` / `INVALID_SESSION` - authentication required
    /// - `CONTRACT_INACTIVE` - caller's contract is not active
    /// </remarks>
    /// <param name="q">Search term (required)</param>
    /// <param name="limit">Maximum results to return, 1-100 (default 20)</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("people")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PersonResult>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SearchPeople(
        [FromQuery] string? q,
        [FromQuery] int limit = 20,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(ApiResponse.Fail("MISSING_QUERY", "Query parameter 'q' is required."));

        if (limit is < 1 or > 100)
            limit = 20;

        var results = await _repo.SearchAsync(q, limit, ct);
        return Ok(ApiResponse<IReadOnlyList<PersonResult>>.Ok(results));
    }
}
