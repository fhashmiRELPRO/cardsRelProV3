using System.Text.Json.Serialization;

namespace RelPro.CARDS.Api.Search.Models;

public sealed class ProspectorSearchResponse
{
    [JsonPropertyName("totalIndividuals")] public int TotalIndividuals                    { get; init; }
    [JsonPropertyName("individuals")]      public IReadOnlyList<IndividualResult> Individuals { get; init; } = [];
    [JsonPropertyName("searchId")]         public int SearchId                            { get; init; }

    public static ProspectorSearchResponse Empty => new() { TotalIndividuals = 0, SearchId = 0 };
}
