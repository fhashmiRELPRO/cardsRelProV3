namespace RelPro.CARDS.Api.User.Repositories;

/// <summary>
/// Internal DTO mapped by Dapper from rcp2_read_user stored procedure output.
/// Property names match the column aliases returned by the proc (case-insensitive).
/// </summary>
public sealed class UserRow
{
    public int     Id                 { get; init; }
    public string  Name               { get; init; } = string.Empty;
    public string? Headline           { get; init; }
    public string  Email              { get; init; } = string.Empty;
    public int     OrganizationId     { get; init; }
    public string? OrganizationName   { get; init; }
    public string? FirstName          { get; init; }
    public string? LastName           { get; init; }
    public bool    Active             { get; init; }
    public bool    Enabled            { get; init; }
    public bool    IsAdmin            { get; init; }
    public string? DateLicenseExpires { get; init; }
}
