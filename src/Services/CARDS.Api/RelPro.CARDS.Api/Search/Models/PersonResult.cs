namespace RelPro.CARDS.Api.Search.Models;

public sealed record PersonResult(
    long Id,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Organization,
    string? Title);
