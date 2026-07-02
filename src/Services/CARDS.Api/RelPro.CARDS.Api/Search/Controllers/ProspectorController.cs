using Microsoft.AspNetCore.Mvc;
using RelPro.Contracts.Common;
using RelPro.CARDS.Api.Search.Models;
using RelPro.CARDS.Api.Search.Repositories;

namespace RelPro.CARDS.Api.Search.Controllers;

[ApiController]
[Route("v1/search")]
[Produces("application/json")]
public sealed class ProspectorController : ControllerBase
{
    private readonly IProspectorSearchRepository _repo;

    public ProspectorController(IProspectorSearchRepository repo) => _repo = repo;

    [HttpGet]
    [ProducesResponseType(typeof(ProspectorSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status408RequestTimeout)]
    public async Task<IActionResult> Search(
        [FromQuery] ProspectorSearchRequest request,
        CancellationToken ct = default)
    {
        if (!request.HasAnySearchTerm)
            return BadRequest(ApiResponse.Fail("MISSING_SEARCH_TERM",
                "At least one search parameter (query, firstName, lastName, orgName, title) is required."));

        var result = await _repo.SearchAsync(request, ct);
        return Ok(result);
    }
}
