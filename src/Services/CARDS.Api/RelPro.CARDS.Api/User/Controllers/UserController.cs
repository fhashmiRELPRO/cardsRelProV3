using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using RelPro.Contracts.Common;
using RelPro.Contracts.Enums;
using RelPro.Infrastructure.Context;
using RelPro.Infrastructure.Entitlements;
using RelPro.CARDS.Api.User.Models;
using RelPro.CARDS.Api.User.Services;

namespace RelPro.CARDS.Api.User.Controllers;

[ApiController]
[Route("v1/user")]
[Produces("application/json")]
public sealed class UserController : ControllerBase
{
    private readonly IRequestContext _ctx;
    private readonly IUserService _userService;
    private readonly string _pendoPrefix;

    public UserController(IRequestContext ctx, IUserService userService, IConfiguration config)
    {
        _ctx         = ctx;
        _userService = userService;
        _pendoPrefix = config["Pendo:AccountIdPrefix"] ?? string.Empty;
    }

    /// <summary>
    /// Returns the profile of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// Reads directly from the request context - no database call required.
    /// The context is populated by `RequestContextMiddleware` which validates the
    /// `userToken` header against the CARDS legacy session store.
    ///
    /// **Error codes:**
    /// - `MISSING_TOKEN` - no `userToken` header present
    /// - `INVALID_SESSION` - token is expired or not recognised
    /// - `CONTRACT_INACTIVE` - the user's contract has expired or been disabled
    /// </remarks>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public IActionResult Me()
    {
        var profile = new UserProfileResponse(
            UserId:           _ctx.UserId,
            OrgId:            _ctx.OrgId,
            ContractId:       _ctx.ContractId,
            Email:            _ctx.UserEmail,
            DisplayName:      _ctx.UserName,
            IsContractActive: _ctx.IsContractActive,
            ContractStatus:   _ctx.ContractStatus.ToString(),
            ContractExpiry:   _ctx.ContractExpiry?.ToString("yyyy-MM-dd"),
            PendoAccountId:   string.IsNullOrEmpty(_pendoPrefix) ? null : $"{_pendoPrefix}{_ctx.ContractId}");

        return Ok(ApiResponse<UserProfileResponse>.Ok(profile));
    }

    /// <summary>
    /// Returns detailed profile of a user by their ID.
    /// </summary>
    /// <remarks>
    /// Requires the **UserManagement** entitlement. The calling user may only retrieve
    /// users that belong to their own organisation - cross-organisation requests return
    /// 404 (not 403) to avoid revealing whether the user exists.
    ///
    /// **Error codes:**
    /// - `INVALID_ID` - id must be a positive integer
    /// - `MISSING_TOKEN` / `INVALID_SESSION` - authentication required
    /// - `ENTITLEMENT_REQUIRED` - caller lacks UserManagement entitlement
    /// - `NOT_FOUND` - user does not exist or belongs to a different organisation
    /// </remarks>
    /// <param name="id">Positive integer user ID</param>
    /// <param name="ct">Cancellation token</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<UserDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct = default)
    {
        if (id <= 0)
            return BadRequest(ApiResponse.Fail("INVALID_ID", "User id must be a positive integer."));

        if (id != _ctx.UserId && !_ctx.HasEntitlement(EntitlementFeature.UserManagement))
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse.Fail("ENTITLEMENT_REQUIRED", "This action requires the 'UserManagement' entitlement."));

        var user = await _userService.GetByIdAsync(_ctx.OrgId, id, ct);
        return Ok(ApiResponse<UserDetailResponse>.Ok(
            user with { PendoAccountId = $"{_pendoPrefix}{_ctx.ContractId}" }));
    }
}
