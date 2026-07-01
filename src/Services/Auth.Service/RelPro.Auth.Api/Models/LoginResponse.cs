namespace RelPro.Auth.Api.Models;

public sealed record LoginResponse(string Token, DateTime ExpiresAt);
