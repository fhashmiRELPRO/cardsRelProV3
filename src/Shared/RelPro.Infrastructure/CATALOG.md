# RelPro.Infrastructure - Pattern Catalog

A reference of the shared database, cache, HTTP, and middleware patterns in this
library. Check here before adding a new one so we reuse the existing pattern. When
you add a pattern, add a matching row so this list stays current.

---

## MySQL (`Database/MySQL/`)

| What you need | Use this |
|---|---|
| Open a MySQL connection | `IMySqlConnectionFactory.CreateAsync()` - inject the factory, call this |
| SELECT multiple rows → list of T | `BaseMySqlRepository.QueryAsync<T>(sql, param)` |
| SELECT one row or null | `BaseMySqlRepository.QuerySingleOrDefaultAsync<T>(sql, param)` |
| INSERT / UPDATE / DELETE | `BaseMySqlRepository.ExecuteAsync(sql, param)` |
| SELECT a single scalar (COUNT, MAX, etc.) | `BaseMySqlRepository.ExecuteScalarAsync<T>(sql, param)` |
| SELECT with pagination → PagedResult<T> | `BaseMySqlRepository.QueryPagedAsync<T>(dataSql, countSql, pagination)` |
| Multiple statements in one transaction | `BaseMySqlRepository.ExecuteTransactionAsync(Func<IDbConnection, IDbTransaction, Task>)` |

**Never**: `new MySqlConnection(connStr)` directly in a repository or service. Always go through the factory.

---

## MongoDB (`Database/MongoDB/`)

| What you need | Use this |
|---|---|
| Get a MongoDB database | `IMongoClientFactory.GetDatabase(databaseName)` |
| Typed collection access | Inherit `BaseMongoRepository<TDocument>` - use `_collection` |
| Find with filter + pagination | `BaseMongoRepository<T>.FindPagedAsync(filter, sort, pagination)` |
| Find one document or null | `BaseMongoRepository<T>.FindOneAsync(filter)` |
| Insert a document | `BaseMongoRepository<T>.InsertAsync(document)` |
| Insert or update (upsert) | `BaseMongoRepository<T>.UpsertAsync(filter, document)` |
| Delete a document | `BaseMongoRepository<T>.DeleteAsync(filter)` |

**Never**: `new MongoClient(connStr)` directly. Always go through `IMongoClientFactory`.

---

## Caching (`Cache/`)

| What you need | Use this |
|---|---|
| Get from cache, set if missing | `ICacheService.GetOrSetAsync<T>(key, factory, ttl)` |
| Invalidate a single key | `ICacheService.RemoveAsync(key)` |
| Invalidate all keys with a prefix | `ICacheService.RemoveByPrefixAsync(prefix)` |
| Raw Redis string get/set | `IDistributedCache` (inject directly) - only for non-standard patterns |

**Never**: Manual `GetStringAsync` → deserialize → `SetStringAsync` sequence. Use `GetOrSetAsync<T>`.

---

## HTTP Clients (`Http/`)

| What you need | Use this |
|---|---|
| Register a typed HttpClient with retry | `services.AddResilientHttpClient<TClient>(name, baseUrl)` - adds Polly (3x retry, exponential backoff) + 5s timeout |
| Make an HTTP GET and deserialize | `HttpClient.GetFromJsonAsync<T>(path)` (built-in System.Net.Http.Json) |
| Make an HTTP POST with JSON body | `HttpClient.PostAsJsonAsync(path, body)` (built-in) |

**Never**: `new HttpClient()` directly. Always use `IHttpClientFactory` or typed client registration.

---

## Request Context (`Context/`)

| What you need | Use this |
|---|---|
| Current user's identity (userId, orgId, contractId) | Inject `IRequestContext` - read `.UserId`, `.OrgId`, `.ContractId` |
| Check if a feature is enabled for this user's contract | `IRequestContext.HasEntitlement(EntitlementFeature.X)` |
| Enforce entitlement on a controller action | `[RequireEntitlement(EntitlementFeature.X)]` attribute |
| Validate a session token | `ISessionValidator.ValidateAsync(token)` |
| Load entitlements by contract | `IEntitlementLoader.LoadAsync(contractId)` |

---

## Error Handling (`Exceptions/`)

| What you need | Use this |
|---|---|
| Feature not available on contract | `throw new EntitlementException(EntitlementFeature.X)` |
| Resource not found | `throw new ResourceNotFoundException("Person", id)` |
| Contract suspended/expired | `throw new ContractInactiveException(contractStatus)` |
| Bad client input | `throw new ArgumentException("message", "paramName")` |

**Never**: `return null` from a service when a resource should exist. Throw `ResourceNotFoundException`.
**Never**: Catch exceptions in controllers to return error responses - the global handler does this.

---

## Middleware (`Middleware/`)

| Middleware | What it does | Registered in |
|---|---|---|
| `RequestContextMiddleware` | Validates session token, populates `IRequestContext` | Every service `Program.cs` |
| `QuotaMiddleware` | Checks quota before request, records usage after | Gateway `Program.cs` |

**Order in pipeline** (must be exact):
1. `UseExceptionHandler`
2. `UseSerilogRequestLogging`
3. `UseMiddleware<RequestContextMiddleware>`
4. `UseRouting`
5. `MapControllers`

---

## DI Registration Helpers (`Extensions/ServiceCollectionExtensions.cs`)

| Method | Registers |
|---|---|
| `services.AddInfrastructure(config)` | All factories, cache, context holder, session validator, entitlement loader |
| `services.AddResilientHttpClient<T>(name, url)` | Typed HttpClient with Polly retry |

Call `services.AddInfrastructure(config)` in every service's `Program.cs` before any service-specific registrations.

---

## Adding a new pattern

1. Implement in the correct file under the matching namespace
2. Add a row to the relevant table above: "what you need" + "use this"
