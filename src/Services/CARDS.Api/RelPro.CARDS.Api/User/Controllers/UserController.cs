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
