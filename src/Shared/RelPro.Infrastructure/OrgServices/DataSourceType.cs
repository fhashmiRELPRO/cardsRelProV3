namespace RelPro.Infrastructure.OrgServices;

/// <summary>
/// Integer constants matching CARDS's DATA_SOURCE_TYPE enum in WEBRequest.cs.
/// </summary>
public static class DataSourceType
{
    public const int MongoDbSearchIndividual   = 2411;
    public const int MongoDbSearchOrg          = 2412;
    public const int MongoDbSearchEducation    = 2414;
    public const int MongoDbSearchUcc          = 2416;
    public const int MongoDbConcordanceOrg     = 2612;
}
