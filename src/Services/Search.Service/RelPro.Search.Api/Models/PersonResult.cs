namespace RelPro.Search.Api.Models;

public sealed record PersonResult(
    long Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Organization,
    string? Title);
