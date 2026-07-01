using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RelPro.Auth.Api.Models;
using RelPro.Auth.Api.Options;
using RelPro.Auth.Api.Services;
using RelPro.Contracts.Common;

namespace RelPro.Auth.Api.Controllers;

[ApiController]
[Route("v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ILoginService _loginService;
    private readonly ContractOptions _contractOpts;

    public AuthController(ILoginService loginService, IOptions<ContractOptions> contractOpts)
    {
        _loginService  = loginService;
        _contractOpts  = contractOpts.Value;
    }

    /// <summary>
    /// Authenticate with email and password and receive a session token.
    /// </summary>
    /// <remarks>
    /// **Development/testing use only during the CARDS migration.**
    ///
    /// Production users authenticate via the CARDS legacy application - their `userToken` cookie/session
    /// is accepted directly by all services without going through this endpoint.
    ///
    /// On success the returned `token` must be sent as the `userToken` header on all subsequent requests.
    ///
    /// **Error codes:**
    /// - `INVALID_CREDENTIALS` - email or password does not match any active account
    /// - `INVALID_ARGUMENT` - email format is invalid or required fields are missing
    /// </remarks>
    /// <param name="request">Login credentials</param>
    /// <param name="ct">Cancellation token</param>
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
