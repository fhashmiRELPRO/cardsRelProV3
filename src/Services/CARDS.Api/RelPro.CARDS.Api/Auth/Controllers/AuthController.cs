using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RelPro.CARDS.Api.Auth.Models;
using RelPro.CARDS.Api.Auth.Options;
using RelPro.CARDS.Api.Auth.Services;
using RelPro.Contracts.Common;

namespace RelPro.CARDS.Api.Auth.Controllers;

[ApiController]
[Route("v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly ContractOptions _contractOpts;

    public AuthController(ILoginService loginService, IOptions<ContractOptions> contractOpts)
    {
        _loginService = loginService;
        _contractOpts = contractOpts.Value;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await _loginService.LoginAsync(request.Email, request.Password, ct);

        if (result.Failure == LoginFailureReason.AccountLocked)
            return StatusCode(StatusCodes.Status429TooManyRequests,
                ApiResponse.Fail("ACCOUNT_LOCKED",
                    $"Too many failed login attempts. Try again in {_contractOpts.FailureTimeMinutes} minutes."));

        if (!result.IsSuccess)
            return Unauthorized(ApiResponse.Fail("INVALID_CREDENTIALS", "Email or password is incorrect."));

        return Ok(ApiResponse<LoginResponse>.Ok(new LoginResponse(result.Token!, result.ExpiresAt!.Value)));
    }
}
