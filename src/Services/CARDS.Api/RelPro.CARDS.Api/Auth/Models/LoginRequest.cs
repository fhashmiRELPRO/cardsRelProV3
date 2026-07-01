using System.ComponentModel.DataAnnotations;

namespace RelPro.CARDS.Api.Auth.Models;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}
