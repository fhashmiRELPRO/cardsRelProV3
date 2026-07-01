using System.Text.Json.Serialization;

namespace RelPro.Search.Api.Models;

/// <summary>
/// Wire-compatible with the CARDS /prospector/v1/search response.
/// No ApiResponse wrapper - frontend consumes this directly.
/// </summary>
public sealed class ProspectorSearchResponse
{
    [JsonPropertyName("totalIndividuals")] public int TotalIndividuals                    { get; init; }
    [JsonPropertyName("individuals")]      public IReadOnlyList<IndividualResult> Individuals { get; init; } = [];
    [JsonPropertyName("searchId")]         public int SearchId                            { get; init; }

    public static ProspectorSearchResponse Empty => new() { TotalIndividuals = 0, SearchId = 0 };
}
