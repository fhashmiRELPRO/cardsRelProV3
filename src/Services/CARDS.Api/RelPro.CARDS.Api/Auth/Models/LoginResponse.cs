namespace RelPro.CARDS.Api.Auth.Models;

public sealed record LoginResponse(string Token, DateTime ExpiresAt);
