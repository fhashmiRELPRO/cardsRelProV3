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
