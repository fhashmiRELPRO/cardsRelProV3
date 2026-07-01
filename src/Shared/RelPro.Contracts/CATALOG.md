# RelPro.Contracts - DTO & Model Catalog

A reference of the shared DTOs, enums, and request/response models in this library.
Check here before adding a new type so we don't create near-duplicates. When you add
a type, add a matching row so this list stays current.

---

## Auth (`Auth/`)

| Type | Solves |
|---|---|
| `LoginRequest` | Email + password login payload |
| `LoginResponse` | Access token, refresh token, MFA session flag |
| `MfaSendRequest` | Trigger MFA code send for a session |
| `MfaVerifyRequest` | Submit MFA code for verification |
| `RefreshRequest` | Submit refresh token to get new access token |
| `SessionInfo` | Internal session data from `ISessionValidator` - not sent to clients |
| `TokenPair` | Access token + refresh token value object |

---

## Users (`Users/`)

| Type | Solves |
|---|---|
| `UserDto` | User identity, state, MFA config, contract link |
| `UserOrgDto` | Organization with license counts and limits |
| `CustomerDto` | Customer billing entity |
| `UserRoleDto` | Role assigned to a user |
| `UserStatDto` | Usage statistics per user |
| `OrgEmailDomainDto` | Allowed email domain for an org |

---

## Contracts (`Contracts/`)

| Type | Solves |
|---|---|
| `ContractDto` | Full contract entity with nested users, entitlements, payment |
| `ContractUserDto` | User membership in a contract |
| `PaymentSourceDto` | Payment method attached to a contract |

---

## Entitlements (`Entitlements/`)

| Type | Solves |
|---|---|
| `ContractEntitlementsDto` | All 76 feature flags for a contract (bool properties) |
| `UserOrgFeatureDto` | Single feature flag record (typeId + name + isAvailable) |
| `EntitlementFeature` (enum) | Enumeration of all 76 feature keys - use this, never raw strings |
| `GdprEuLevel` (enum) | GDPR compliance level (Level1, Level2, Level3) |

---

## Quota (`Quota/`)

| Type | Solves |
|---|---|
| `UserQuotaDto` | Per-user quota with period, usage, and limit tracking |
| `OrgQuotaDto` | Organization-level quota |
| `ContractQuotaDto` | Contract-level quota |
| `QuotaCheckRequest` | Payload for gateway quota pre-check |
| `QuotaCheckResult` | Quota check outcome: allowed, exceeded flags, remaining, reset time |
| `QuotaRecordRequest` | Payload for gateway quota usage recording |
| `QuotaPeriod` (enum) | OneTime, Hourly, Daily, Weekly, Monthly, Quarterly, Annually |
| `QuotaType` (enum) | All, OrgDefault, UserDefaultForOrg, UserDefaultForAll |
| `QuotaSourceType` (enum) | OrgQuota, UserQuota, ContractQuota |
| `QuotaProfileType` (enum) | AllProfiles, IndividualOnly, OrgOnly |

---

## Common Wrappers (`Common/`)

| Type | Solves |
|---|---|
| `ApiResponse<T>` | Standard success/failure envelope - use for ALL controller responses |
| `ApiResponse<T>.Ok(data)` | Build a success response |
| `ApiResponse<T>.Fail(message, code)` | Build an error response |
| `PagedResult<T>` | Paginated result set with Items, TotalCount, Page, PageSize, TotalPages |
| `ErrorResponse` | Standard error shape used in global exception handler |

---

## Enums (`Enums/`)

| Enum | Values |
|---|---|
| `UserState` | Signup, Verified, Approved, ResetPwd, InviteSent, MustResetPwd, Locked |
| `ContractStatus` | Active, Completed, Canceled, Suspended |
| `PaymentType` | FreeTrial, Invoice, CreditCard |
| `PaymentStatus` | NewContract, Authorized, Received, Rejected, Delinquent, Settled, Late |
| `LicenseType` | PurchaseOrder, CreditCard |
| `MfaMethod` | Email, SMS |

---

## Context (`Context/`)

| Type | Solves |
|---|---|
| `IRequestContext` | Interface for all caller identity and entitlement access in services |

---

## Adding a new DTO

1. Determine which folder it belongs in above
2. Create the file in that folder with only plain C# properties (no base classes, no enum-indexed backing store)
3. Add it to the relevant table above: type name + one-line description
