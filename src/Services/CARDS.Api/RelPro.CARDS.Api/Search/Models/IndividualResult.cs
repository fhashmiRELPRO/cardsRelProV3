using System.Text.Json.Serialization;

namespace RelPro.CARDS.Api.Search.Models;

public sealed class IndividualResult
{
    [JsonPropertyName("firstName")]    public string  FirstName    { get; init; } = "";
    [JsonPropertyName("nickName")]     public string? NickName     { get; init; }
    [JsonPropertyName("middleName")]   public string? MiddleName   { get; init; }
    [JsonPropertyName("lastName")]     public string  LastName     { get; init; } = "";
    [JsonPropertyName("role")]         public string? Role         { get; init; }
    [JsonPropertyName("roleStartDate")]public string? RoleStartDate{ get; init; }
    [JsonPropertyName("roleEndDate")]  public string? RoleEndDate  { get; init; }
    [JsonPropertyName("companyId")]    public int     CompanyId    { get; init; }
    [JsonPropertyName("company")]      public string? Company      { get; init; }
    [JsonPropertyName("city")]         public string? City         { get; init; }
    [JsonPropertyName("state")]        public string? State        { get; init; }
    [JsonPropertyName("country")]      public string? Country      { get; init; }
    [JsonPropertyName("zipcode")]      public string? Zipcode      { get; init; }
    [JsonPropertyName("rcpId")]        public int     RcpId        { get; init; }
    [JsonPropertyName("currentFlag")]  public int     CurrentFlag  { get; init; }
    [JsonPropertyName("employees")]    public int     Employees    { get; init; }
    [JsonPropertyName("revenue")]      public int     Revenue      { get; init; }
    [JsonPropertyName("lastUpdated")]  public string? LastUpdated  { get; init; }
    [JsonPropertyName("isEU")]         public bool    IsEU         { get; init; }
    [JsonPropertyName("hasWealth")]    public bool    HasWealth    { get; init; }
    [JsonPropertyName("publicCompany")]public bool    PublicCompany{ get; init; }
}
