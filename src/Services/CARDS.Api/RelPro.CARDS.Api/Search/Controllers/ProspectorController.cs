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

    /// <summary>
    /// Individual (people) prospector search via MongoDB Atlas Search.
    /// </summary>
    /// <remarks>
    /// **Wire-compatible with `GET /prospector/v1/search` on the legacy CARDS application.**
    /// The response shape is identical - frontend clients can point here with no code changes.
    ///
    /// At least one search term must be supplied (`query`, `firstName`, `lastName`, `orgName`, or `title`).
    /// The MongoDB database and collection names are resolved at runtime from `user_org_services`
    /// per the caller's `orgId` - no configuration change is needed when a vendor refreshes their dataset.
    ///
    /// `pageSize` is clamped to [1, 100]; `page` is 1-based and defaults to 1.
    ///
    /// **Error codes:**
    /// - `MISSING_SEARCH_TERM` - at least one search parameter is required
    /// - `MISSING_TOKEN` / `INVALID_SESSION` - authentication required
    /// - `ENTITLEMENT_REQUIRED` - org has no MongoDbIndividualSearch service configured
    /// - `REQUEST_TIMEOUT` - Atlas Search did not respond in time (equivalent to CARDS HTTP 408)
    /// </remarks>
    /// <param name="request">Search parameters</param>
    /// <param name="ct">Cancellation token</param>
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
