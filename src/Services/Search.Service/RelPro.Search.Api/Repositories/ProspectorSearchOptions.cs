namespace RelPro.Search.Api.Repositories;

public sealed class ProspectorSearchOptions
{
    /// <summary>MongoDB Atlas Search index name. CARDS uses "default".</summary>
    public string AtlasIndexName { get; init; } = "default";
}
