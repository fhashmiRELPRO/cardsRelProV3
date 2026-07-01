using Microsoft.AspNetCore.Mvc;

namespace RelPro.Search.Api.Models;

public sealed class ProspectorSearchRequest
{
    [FromQuery(Name = "query")]                    public string? Query                    { get; init; }
    [FromQuery(Name = "firstName")]                public string? FirstName                { get; init; }
    [FromQuery(Name = "lastName")]                 public string? LastName                 { get; init; }
    [FromQuery(Name = "orgName")]                  public string? OrgName                  { get; init; }
    [FromQuery(Name = "title")]                    public string? Title                    { get; init; }
    [FromQuery(Name = "pageSize")]                 public int PageSize                     { get; init; } = 20;
    [FromQuery(Name = "page")]                     public int Page                         { get; init; } = 1;
    [FromQuery(Name = "showPastRole")]             public bool ShowPastRole                { get; init; } = false;
    [FromQuery(Name = "includeInActiveCompanies")] public bool IncludeInActiveCompanies    { get; init; } = false;
    [FromQuery(Name = "hasEMails")]                public bool? HasEmails                  { get; init; }
    [FromQuery(Name = "hasDirectPhone")]           public bool? HasDirectPhone             { get; init; }
    [FromQuery(Name = "hasMobilePhone")]           public bool? HasMobilePhone             { get; init; }
    [FromQuery(Name = "hasCV")]                    public bool? HasCv                      { get; init; }
    [FromQuery(Name = "searchId")]                 public int SearchId                     { get; init; } = 0;

    public bool HasAnySearchTerm =>
        !string.IsNullOrWhiteSpace(Query) ||
        !string.IsNullOrWhiteSpace(FirstName) ||
        !string.IsNullOrWhiteSpace(LastName) ||
        !string.IsNullOrWhiteSpace(OrgName) ||
        !string.IsNullOrWhiteSpace(Title);

    public int SafePageSize => PageSize is >= 1 and <= 100 ? PageSize : 20;
    public int SafePage     => Page >= 1 ? Page : 1;
}
