using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using RelPro.Common.Exceptions;
using RelPro.Infrastructure.Context;
using RelPro.Infrastructure.Database;
using RelPro.Infrastructure.OrgServices;
using RelPro.CARDS.Api.Search.Models;

namespace RelPro.CARDS.Api.Search.Repositories;

public sealed class ProspectorSearchRepository : IProspectorSearchRepository
{
    private readonly IMongoClientFactory _mongo;
    private readonly IUserOrgServiceRepository _orgServices;
    private readonly IRequestContext _ctx;
    private readonly string _atlasIndexName;

    public ProspectorSearchRepository(
        IMongoClientFactory mongo,
        IUserOrgServiceRepository orgServices,
        IRequestContext ctx,
        IOptions<ProspectorSearchOptions> opts)
    {
        _mongo          = mongo;
        _orgServices    = orgServices;
        _ctx            = ctx;
        _atlasIndexName = opts.Value.AtlasIndexName;
    }

    private async Task<IMongoCollection<BsonDocument>> GetCollectionAsync(CancellationToken ct)
    {
        var svc = await _orgServices.GetAsync(_ctx.OrgId, DataSourceType.MongoDbSearchIndividual, ct)
            ?? throw new EntitlementException("MongoDbIndividualSearch");

        var db  = svc.Root            ?? throw new InvalidOperationException($"user_org_services.root is null for org {_ctx.OrgId}");
        var col = svc.ProfileEndpoint ?? throw new InvalidOperationException($"user_org_services.profile_endpoint is null for org {_ctx.OrgId}");

        return _mongo.GetDatabase(db).GetCollection<BsonDocument>(col);
    }

    public async Task<ProspectorSearchResponse> SearchAsync(
        ProspectorSearchRequest request, CancellationToken ct = default)
    {
        var collection = await GetCollectionAsync(ct);
        var pipeline   = BuildPipeline(request);

        var cursor = await collection.AggregateAsync<BsonDocument>(pipeline,
            new AggregateOptions { AllowDiskUse = true }, ct);

        var facetDoc = await cursor.FirstOrDefaultAsync(ct);
        if (facetDoc is null) return ProspectorSearchResponse.Empty;

        var totalArray = facetDoc["total"].AsBsonArray;
        var total      = totalArray.Count > 0 ? totalArray[0]["count"].AsInt32 : 0;

        var results = facetDoc["results"].AsBsonArray
            .Select(MapIndividual)
            .ToList();

        return new ProspectorSearchResponse
        {
            TotalIndividuals = total,
            Individuals      = results,
            SearchId         = 0
        };
    }

    private BsonDocument[] BuildPipeline(ProspectorSearchRequest req)
    {
        var mustClauses = new BsonArray();

        var searchText = BuildSearchText(req);
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            mustClauses.Add(new BsonDocument("text", new BsonDocument
            {
                { "query", searchText },
                { "path",  new BsonArray { "first_name", "last_name", "title", "org_name" } }
            }));
        }

        var filterClauses = new BsonArray();

        if (!req.ShowPastRole)
        {
            filterClauses.Add(new BsonDocument("equals", new BsonDocument
            {
                { "path",  "current_flag" },
                { "value", 1 }
            }));
        }

        if (!req.IncludeInActiveCompanies)
        {
            filterClauses.Add(new BsonDocument("equals", new BsonDocument
            {
                { "path",  "company_active" },
                { "value", true }
            }));
        }

        var compound = new BsonDocument();
        if (mustClauses.Count   > 0) compound["must"]   = mustClauses;
        if (filterClauses.Count > 0) compound["filter"] = filterClauses;

        var searchStage = new BsonDocument("$search", new BsonDocument
        {
            { "index",    _atlasIndexName },
            { "compound", compound }
        });

        var skip = (req.SafePage - 1) * req.SafePageSize;

        var sortDoc = new BsonDocument
        {
            { "position_id",    1 },
            { "source_id",      1 },
            { "board_member",   1 },
            { "hierarchy_lvl", -1 },
            { "management_lvl", 1 },
            { "revenue",       -1 },
            { "current_flag",   1 }
        };

        var projectDoc = new BsonDocument
        {
            { "firstName",    new BsonDocument("$ifNull", new BsonArray { "$first_name",   "" }) },
            { "nickName",     "$nick_name"      },
            { "middleName",   "$middle_name"    },
            { "lastName",     new BsonDocument("$ifNull", new BsonArray { "$last_name",    "" }) },
            { "role",         "$title"          },
            { "roleStartDate","$role_start_date" },
            { "roleEndDate",  "$role_end_date"  },
            { "companyId",    new BsonDocument("$ifNull", new BsonArray { "$company_id",   0  }) },
            { "company",      "$org_name"       },
            { "city",         "$city"           },
            { "state",        "$state"          },
            { "country",      "$country"        },
            { "zipcode",      "$zipcode"        },
            { "rcpId",        new BsonDocument("$ifNull", new BsonArray { "$rcp_id",       0  }) },
            { "currentFlag",  new BsonDocument("$ifNull", new BsonArray { "$current_flag", 0  }) },
            { "employees",    new BsonDocument("$ifNull", new BsonArray { "$employees",    0  }) },
            { "revenue",      new BsonDocument("$ifNull", new BsonArray { "$revenue",      0  }) },
            { "lastUpdated",  "$last_updated"   },
            { "isEU",         new BsonDocument("$eq",  new BsonArray { "$eu_flag",          1     }) },
            { "hasWealth",    new BsonDocument("$eq",  new BsonArray { "$has_wealth",       true  }) },
            { "publicCompany",new BsonDocument("$eq",  new BsonArray { "$public_company",   true  }) },
            { "_id",          0 }
        };

        var facetStage = new BsonDocument("$facet", new BsonDocument
        {
            { "total",   new BsonArray { new BsonDocument("$count", "count") } },
            { "results", new BsonArray
                {
                    new BsonDocument("$sort",    sortDoc),
                    new BsonDocument("$skip",    skip),
                    new BsonDocument("$limit",   req.SafePageSize),
                    new BsonDocument("$project", projectDoc)
                }
            }
        });

        return [searchStage, facetStage];
    }

    private static string BuildSearchText(ProspectorSearchRequest req)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(req.Query))     parts.Add(req.Query.Trim());
        if (!string.IsNullOrWhiteSpace(req.FirstName)) parts.Add(req.FirstName.Trim());
        if (!string.IsNullOrWhiteSpace(req.LastName))  parts.Add(req.LastName.Trim());
        if (!string.IsNullOrWhiteSpace(req.OrgName))   parts.Add(req.OrgName.Trim());
        if (!string.IsNullOrWhiteSpace(req.Title))     parts.Add(req.Title.Trim());
        return string.Join(" ", parts);
    }

    private static IndividualResult MapIndividual(BsonValue doc)
    {
        static string  S(BsonValue v) => v.IsBsonNull ? "" : v.AsString;
        static string? N(BsonValue v) => v.IsBsonNull ? null : v.AsString;
        static int     I(BsonValue v) => v.IsBsonNull ? 0  : v.ToInt32();
        static bool    B(BsonValue v) => !v.IsBsonNull && v.AsBoolean;

        return new IndividualResult
        {
            FirstName     = S(doc["firstName"]),
            NickName      = N(doc["nickName"]),
            MiddleName    = N(doc["middleName"]),
            LastName      = S(doc["lastName"]),
            Role          = N(doc["role"]),
            RoleStartDate = N(doc["roleStartDate"]),
            RoleEndDate   = N(doc["roleEndDate"]),
            CompanyId     = I(doc["companyId"]),
            Company       = N(doc["company"]),
            City          = N(doc["city"]),
            State         = N(doc["state"]),
            Country       = N(doc["country"]),
            Zipcode       = N(doc["zipcode"]),
            RcpId         = I(doc["rcpId"]),
            CurrentFlag   = I(doc["currentFlag"]),
            Employees     = I(doc["employees"]),
            Revenue       = I(doc["revenue"]),
            LastUpdated   = N(doc["lastUpdated"]),
            IsEU          = B(doc["isEU"]),
            HasWealth     = B(doc["hasWealth"]),
            PublicCompany = B(doc["publicCompany"]),
        };
    }
}
