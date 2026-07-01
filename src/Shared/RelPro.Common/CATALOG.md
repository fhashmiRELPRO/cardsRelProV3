# RelPro.Common - Utility Catalog

A quick reference of the shared helpers in this library. Check here before adding a
new utility so we don't end up with duplicates. When you add a helper, add a matching
row so this list stays current.

---

## Strings (`Strings/StringExtensions.cs`)

| Method | Solves |
|---|---|
| `.TruncateWithEllipsis(int max)` | Shorten display text without cutting mid-word |
| `.ToSlug()` | URL-safe lowercase slug from a name or title |
| `.NormalizeWhitespace()` | Collapse multiple spaces/newlines into a single space |
| `.StripHtml()` | Remove all HTML tags from a string |
| `.MaskEmail()` | Partially obscure email for display (`j***@company.com`) |
| `.ContainsAny(IEnumerable<string>)` | True if string contains any item in the list |

**Do not implement**: `IsNullOrWhiteSpace` → use `string.IsNullOrWhiteSpace()` (BCL)

---

## Dates (`Dates/DateExtensions.cs`)

| Method | Solves |
|---|---|
| `.ToUtcDateOnly()` | Convert DateTime to DateOnly in UTC |
| `.StartOfDay()` | Midnight (00:00:00 UTC) of the given date - for range queries |
| `.EndOfDay()` | 23:59:59.999 UTC of the given date - for range queries |
| `.StartOfMonth()` | First instant of the calendar month |
| `.EndOfMonth()` | Last instant of the calendar month |
| `.ToIso8601()` | Format as ISO 8601 string (`2026-06-09T14:30:00Z`) |
| `.IsExpired()` | True if DateOnly is before today (UTC) |
| `.ToRelativeDisplay()` | Human-readable "3 days ago" / "in 2 weeks" |

---

## Numbers (`Numbers/NumberExtensions.cs`)

| Method | Solves |
|---|---|
| `.ToOrdinal()` | Convert int to "1st", "2nd", "3rd" |
| `.Clamp(min, max)` | Keep a value within a range |
| `.IsPositive()` | Null-safe positive check on int?/decimal? |
| `.IsNegative()` | Null-safe negative check on int?/decimal? |

---

## Collections (`Collections/CollectionExtensions.cs`)

| Method | Solves |
|---|---|
| `.ToPagedResult(page, pageSize)` | Slice IEnumerable into `PagedResult<T>` |
| `.IsNullOrEmpty()` | Null-safe empty check on any IEnumerable |
| `.Batch(int size)` | Split list into chunks of N for bulk ops |

**Do not implement**: `DistinctBy` → use LINQ built-in `.DistinctBy()` (.NET 6+)

---

## Security (`Security/`)

| Class / Method | Solves |
|---|---|
| `PasswordHasher.Hash(password, salt)` | Hash a password using the same algorithm as the legacy CARDS app |
| `PasswordHasher.Verify(password, hash, salt)` | Verify a password against a stored hash |
| `TokenGenerator.Generate(int length)` | Cryptographically random URL-safe token string |

---

## Models (`Models/`)

| Type | Solves |
|---|---|
| `PaginationRequest` | Inbound `Page` + `PageSize` (defaults: page=1, size=25, max=100) |
| `SortRequest` | Inbound `SortBy` + `SortDirection` |
| `PagedResult<T>` | Outbound `Items` + `TotalCount` + `Page` + `PageSize` + `TotalPages` |

---

## Constants (`Constants/AppConstants.cs`)

| Constant | Value | Solves |
|---|---|---|
| `MaxPageSize` | 100 | Maximum page size accepted from clients |
| `DefaultPageSize` | 25 | Default page size when not specified |
| `SessionCacheTtlMinutes` | 5 | Redis TTL for cached session validations |
| `EntitlementCacheTtlMinutes` | 20 | Redis TTL for cached entitlement sets |

---

## Adding a new utility

1. Implement it in the correct file under the matching folder
2. Add one row to the table above: name + what problem it solves
