---
title: CRM — Company Entity (Core Model)
version: 1.0
date_created: 2026-02-24
last_updated: 2026-02-24
owner: JobHunter07 API Team
tags: schema, crm, company, data, api, cqrs, vertical-slice
---

# Introduction

This specification defines the foundational `Company` entity for the CRM (Customer Relationship Management) module of the JobHunter07 API. It covers the database schema, data contracts, CRUD API surface, service layer responsibilities, repository interface, validation rules, and test strategy required to implement the Company entity as a self-contained vertical slice.

---

## 1. Purpose & Scope

**Purpose:**  
Define all requirements, data contracts, validation rules, and acceptance criteria for the `Company` entity so that a developer or AI agent can implement the feature unambiguously and consistently with the existing Vertical Slice Architecture.

**Scope:**  
This specification covers:
- The `Company` database table and EF Core migration
- Request/response DTO contracts
- Minimal API endpoint definitions (`POST`, `GET`, `GET list`, `PUT`, `DELETE`)
- Service layer (`CompanyService`) responsibilities
- Repository interface (`ICompanyRepository`)
- FluentValidation rules
- Unit and integration test requirements

**Out of scope (separate User Stories):**
- Sync metadata and sync filters
- Cloudflare Tunnel integration
- Worker service integration
- Peer discovery and decentralized sync logic

**Intended audience:** Developers and AI coding agents implementing or reviewing the CRM Company feature.

**Assumptions:**
- The project uses .NET 10, ASP.NET Core Minimal APIs, EF Core with SQL Server/PostgreSQL, and FluentValidation.
- The Vertical Slice Architecture pattern and CQRS handler pipeline already exist in the codebase.
- The existing `IRepository<T>`, `IUnitOfWork`, `IHandler<,>`, `IApiEndpoint`, and `Result<T>` abstractions are reused.

---

## 2. Definitions

| Term | Definition |
|---|---|
| **CRM** | Customer Relationship Management — the module that manages companies, contacts, and job applications. |
| **Company** | An organization that is a potential employer or professional contact tracked in the system. |
| **Vertical Slice** | A self-contained feature unit containing its own endpoint, handler, validator, and DTOs. |
| **CQRS** | Command Query Responsibility Segregation — pattern separating read (query) and write (command) operations. |
| **Handler** | A class implementing `IHandler<TRequest, TResponse>` that contains the business logic for a single operation. |
| **DTO** | Data Transfer Object — a record used to move data between the API boundary and the service layer. |
| **Soft Delete** | Marking a record as inactive (`IsActive = false`) rather than physically removing it from the database. |
| **UUIDv7** | A time-ordered UUID variant (`Guid.CreateVersion7()`) used as the primary key for sortability. |
| **Result&lt;T&gt;** | A discriminated union wrapper indicating success (with value) or failure (with `Error`). |
| **EF Core** | Entity Framework Core — the ORM used for database access. |
| **PK** | Primary Key. |

---

## 3. Requirements, Constraints & Guidelines

### Data Model Requirements

- **REQ-001**: The `Company` entity MUST have the following fields: `CompanyId` (UUIDv7, PK), `Name` (nvarchar 200, required), `Domain` (nvarchar 200, optional), `Description` (nvarchar max, optional), `Industry` (nvarchar 100, optional), `WebsiteUrl` (nvarchar 300, optional), `LinkedInUrl` (nvarchar 300, optional), `CreatedAt` (datetime, required), `UpdatedAt` (datetime, required), `IsActive` (bit, required, default `true`).
- **REQ-002**: `CreatedAt` and `UpdatedAt` MUST be set automatically; `UpdatedAt` MUST be updated on every write operation.
- **REQ-003**: `CompanyId` MUST be generated using `Guid.CreateVersion7()` at creation time.

### Database Constraints

- **CON-001**: `Name` MUST have a unique index on `LOWER(Name)` (case-insensitive uniqueness).
- **CON-002**: `Domain` MUST have a unique filtered index where value is not null.
- **CON-003**: The migration MUST use the existing EF Core migration system and be named descriptively (e.g., `AddCompaniesTable`).
- **CON-004**: The `Companies` table MUST be registered in `ApplicationDbContext` as `DbSet<Company>`.

### API Requirements

- **REQ-004**: The following endpoints MUST be implemented:

  | Method | Route | Description |
  |---|---|---|
  | `POST` | `/crm/companies` | Create a new company |
  | `GET` | `/crm/companies/{id}` | Get a company by ID |
  | `GET` | `/crm/companies` | Search/list companies (paginated) |
  | `PUT` | `/crm/companies/{id}` | Update an existing company |
  | `DELETE` | `/crm/companies/{id}` | Soft-delete a company |

- **REQ-005**: All endpoints MUST be tagged with `"companies"` in OpenAPI documentation.
- **REQ-006**: Endpoints MUST produce typed `Produces<T>()` annotations for all response types.
- **REQ-007**: Endpoints MUST delegate all business logic to the handler/service layer — no business logic in endpoint classes.

### Service Layer Requirements

- **REQ-008**: `CompanyService` MUST implement the following methods: `CreateAsync`, `GetByIdAsync`, `SearchAsync`, `UpdateAsync`, `SoftDeleteAsync`.
- **REQ-009**: `CompanyService` MUST enforce uniqueness rules (name case-insensitive, domain) and throw domain-specific errors using `Error` types, not generic exceptions.
- **REQ-010**: `CompanyService` MUST NOT contain controller or HTTP-specific logic.

### Repository Requirements

- **REQ-011**: `ICompanyRepository` MUST define: `AddAsync`, `GetByIdAsync`, `SearchAsync`, `UpdateAsync`, `SoftDeleteAsync`.
- **REQ-012**: `CompanyRepository` MUST use the existing `ApplicationDbContext` and follow the same patterns as `Repository<T>`.
- **REQ-013**: Repository methods MUST NOT contain business or validation logic.

### Validation Requirements

- **REQ-014**: `Name` MUST be required, minimum 2 characters, maximum 200 characters.
- **REQ-015**: `Name` uniqueness (case-insensitive) MUST be enforced in the service layer; the validator handles format rules only.
- **REQ-016**: `Domain` is optional. If provided, it MUST be unique; MUST not exceed 200 characters.
- **REQ-017**: `WebsiteUrl`, if provided, MUST be a valid absolute URL (`http://` or `https://`).
- **REQ-018**: `LinkedInUrl`, if provided, MUST match the pattern `https://www.linkedin.com/company/`.

### Structural Constraints

- **CON-005**: All feature files MUST reside under `Features/Crm/Companies/` within the `JobHunter07.API` project, using sub-folders per operation (e.g., `CreateCompany/`, `GetCompanyById/`, etc.).
- **CON-006**: DTOs MUST be defined as C# `record` types with XML doc comments.
- **CON-007**: Internal entity fields (`CreatedAt`, `UpdatedAt`) MUST NOT be settable from request DTOs. `IsActive` MAY be included in response DTOs for administrative consumers.
- **CON-008**: The `Company` entity class MUST reside in `Entities/` and MUST NOT contain business logic.

### Guidelines

- **GUD-001**: Follow the existing `IHandler<TRequest, TResponse>` + `Result<T>` pattern for all handlers.
- **GUD-002**: Use `Result.Success(...)` and `Result.Failure<T>(...)` consistently.
- **GUD-003**: Errors MUST use named `Error` factory methods (`Error.NotFound`, `Error.Conflict`, `Error.Validation`, etc.) defined in a `CompanyErrors` static class.
- **GUD-004**: Validators MUST extend `AbstractValidator<T>` and be auto-discovered by the assembly scanner.
- **GUD-005**: Pagination for the search endpoint MUST accept `page` (int, min 1) and `pageSize` (int, min 1, max 100, default 20) query parameters.

### Patterns

- **PAT-001**: Each operation (Create, GetById, Search, Update, Delete) is a separate handler class — single responsibility.
- **PAT-002**: Requests and responses are co-located `record` types in the same file as the handler.
- **PAT-003**: The endpoint class is `internal sealed` and implements `IApiEndpoint`.

---

## 4. Interfaces & Data Contracts

### 4.1 Company Entity

```csharp
// Entities/Company.cs
public sealed class Company
{
    public Guid CompanyId { get; set; }          // UUIDv7, PK
    public required string Name { get; set; }    // nvarchar(200), unique (case-insensitive)
    public string? Domain { get; set; }          // nvarchar(200), unique nullable
    public string? Description { get; set; }     // nvarchar(max)
    public string? Industry { get; set; }        // nvarchar(100)
    public string? WebsiteUrl { get; set; }      // nvarchar(300)
    public string? LinkedInUrl { get; set; }     // nvarchar(300)
    public DateTime CreatedAt { get; set; }      // required, UTC
    public DateTime UpdatedAt { get; set; }      // required, UTC, updated on every write
    public bool IsActive { get; set; } = true;   // soft-delete flag
}
```

### 4.2 DTOs

```csharp
/// <summary>Request payload to create a new company.</summary>
public sealed record CreateCompanyRequest(
    string Name,
    string? Domain,
    string? Description,
    string? Industry,
    string? WebsiteUrl,
    string? LinkedInUrl
);

/// <summary>Request payload to update an existing company. Null fields are ignored (partial update).</summary>
public sealed record UpdateCompanyRequest(
    Guid CompanyId,
    string? Name,
    string? Domain,
    string? Description,
    string? Industry,
    string? WebsiteUrl,
    string? LinkedInUrl
);

/// <summary>Search/list query parameters.</summary>
public sealed record SearchCompaniesRequest(
    string? Name,
    string? Domain,
    string? Industry,
    int Page = 1,
    int PageSize = 20
);

/// <summary>Standard company response returned from all read and write operations.</summary>
public sealed record CompanyResponse(
    Guid CompanyId,
    string Name,
    string? Domain,
    string? Description,
    string? Industry,
    string? WebsiteUrl,
    string? LinkedInUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive
);

/// <summary>Paginated list of companies.</summary>
public sealed record SearchCompaniesResponse(
    IEnumerable<CompanyResponse> Companies,
    int Page,
    int PageSize,
    int TotalCount
);

/// <summary>Minimal response for delete operations.</summary>
public sealed record DeleteCompanyResponse(Guid CompanyId);
```

### 4.3 ICompanyRepository Interface

```csharp
public interface ICompanyRepository
{
    Task<Company> AddAsync(Company company, CancellationToken cancellationToken = default);
    Task<Company?> GetByIdAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<(IEnumerable<Company> Items, int TotalCount)> SearchAsync(
        string? name, string? domain, string? industry,
        int page, int pageSize,
        CancellationToken cancellationToken = default);
    Task UpdateAsync(Company company, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(Company company, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDomainAsync(string domain, Guid? excludeId, CancellationToken cancellationToken = default);
}
```

### 4.4 API Endpoint Contracts

| Method | Route | Request | Success Response | Error Responses |
|---|---|---|---|---|
| `POST` | `/crm/companies` | `CreateCompanyRequest` (JSON body) | `200 CompanyResponse` | `400 ValidationError`, `409 Conflict` |
| `GET` | `/crm/companies/{id:guid}` | `id` route param | `200 CompanyResponse` | `404 NotFound` |
| `GET` | `/crm/companies` | `name`, `domain`, `industry`, `page`, `pageSize` (query) | `200 SearchCompaniesResponse` | `400 ValidationError` |
| `PUT` | `/crm/companies/{id:guid}` | `UpdateCompanyRequest` (JSON body, `id` from route) | `200 CompanyResponse` | `400`, `404`, `409` |
| `DELETE` | `/crm/companies/{id:guid}` | `id` route param | `200 DeleteCompanyResponse` | `404 NotFound` |

### 4.5 Error Codes

All domain errors MUST be defined in `CompanyErrors` following the pattern of the existing `BookErrors`:

| Error Code | Type | Description |
|---|---|---|
| `Companies.NotFound` | `ErrorType.NotFound` | Company with the specified ID does not exist or is inactive |
| `Companies.NameConflict` | `ErrorType.Conflict` | A company with the same name (case-insensitive) already exists |
| `Companies.DomainConflict` | `ErrorType.Conflict` | A company with the same domain already exists |

### 4.6 EF Core Model Configuration

The `OnModelCreating` override in `ApplicationDbContext` MUST include the following for `Company`:

```csharp
modelBuilder.Entity<Company>(entity =>
{
    entity.HasKey(e => e.CompanyId);
    entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Domain).HasMaxLength(200);
    entity.Property(e => e.Industry).HasMaxLength(100);
    entity.Property(e => e.WebsiteUrl).HasMaxLength(300);
    entity.Property(e => e.LinkedInUrl).HasMaxLength(300);

    // Unique case-insensitive index on Name
    entity.HasIndex(e => e.Name)
          .IsUnique()
          .HasDatabaseName("IX_Companies_Name_CI");

    // Filtered unique index on Domain (allows multiple NULLs)
    entity.HasIndex(e => e.Domain)
          .IsUnique()
          .HasFilter("[Domain] IS NOT NULL")
          .HasDatabaseName("IX_Companies_Domain");
});
```

---

## 5. Acceptance Criteria

- **AC-001**: Given a valid `CreateCompanyRequest`, when `POST /crm/companies` is called, then a `200 OK` with a populated `CompanyResponse` is returned; `CreatedAt` and `UpdatedAt` are UTC now; `IsActive` is `true`; `CompanyId` is a non-empty UUIDv7.
- **AC-002**: Given a `CreateCompanyRequest` where `Name` matches an existing company name (case-insensitive), when `POST /crm/companies` is called, then `409 Conflict` with error code `Companies.NameConflict` is returned.
- **AC-003**: Given a `CreateCompanyRequest` where `Domain` matches an existing company domain, when `POST /crm/companies` is called, then `409 Conflict` with error code `Companies.DomainConflict` is returned.
- **AC-004**: Given a valid existing `CompanyId`, when `GET /crm/companies/{id}` is called, then `200 OK` with the correct `CompanyResponse` is returned.
- **AC-005**: Given a non-existent `CompanyId`, when `GET /crm/companies/{id}` is called, then `404 Not Found` with error code `Companies.NotFound` is returned.
- **AC-006**: Given optional query parameters, when `GET /crm/companies` is called, then only matching active companies are returned, paginated correctly, with `TotalCount` reflecting the filtered result count.
- **AC-007**: Given a `PUT /crm/companies/{id}` with only `Name` set and other fields null, then only `Name` is updated; all other fields retain prior values; `UpdatedAt` is refreshed.
- **AC-008**: Given a `DELETE /crm/companies/{id}` for an active company, when processed, then `IsActive` is set to `false`, `UpdatedAt` is refreshed, `200 OK` with `DeleteCompanyResponse` is returned, and the record is NOT physically deleted.
- **AC-009**: Given a soft-deleted company, when `GET /crm/companies/{id}` is called, then `404 Not Found` is returned.
- **AC-010**: Given `Name` is empty or fewer than 2 characters, when `POST /crm/companies` is called, then `400 Bad Request` with validation errors is returned.
- **AC-011**: Given `WebsiteUrl` is not a valid absolute URL, when `POST /crm/companies` is called, then `400 Bad Request` with a descriptive validation error is returned.
- **AC-012**: Given `LinkedInUrl` does not start with `https://www.linkedin.com/company/`, when `POST /crm/companies` is called, then `400 Bad Request` with a descriptive validation error is returned.
- **AC-013**: Given any request, when the handler pipeline processes it, then the `ValidationDecorator` and `LoggingDecorator` are applied in the correct order before the handler executes.

---

## 6. Test Automation Strategy

- **Test Levels**: Unit tests and Integration tests.
- **Frameworks**: xUnit (preferred) or MSTest; Moq for unit test mocking.
- **Test project location**: `JobHunter07.API.Tests` project (create if absent) with sub-folders `Unit/Crm/Companies/` and `Integration/Crm/Companies/`.

### Unit Tests

Unit tests MUST cover:
- `CreateCompanyValidator`: all validation rules (name required, min/max length, URL format, LinkedIn URL format).
- `UpdateCompanyValidator`: same field-level rules where applicable.
- `CompanyService.CreateAsync`: name conflict detection, domain conflict detection, successful creation path.
- `CompanyService.UpdateAsync`: not-found path, name conflict on update, partial update field preservation.
- `CompanyService.SoftDeleteAsync`: not-found path, successful soft-delete path.

### Integration Tests

Integration tests MUST use a real test database (not mocked) and MUST cover:
- Create company — end-to-end HTTP `POST /crm/companies`.
- Retrieve company by ID — end-to-end HTTP `GET /crm/companies/{id}`.
- Update company — end-to-end HTTP `PUT /crm/companies/{id}`.
- Soft delete company — end-to-end HTTP `DELETE /crm/companies/{id}` and verify `IsActive = false` in database.
- Search companies by name filter, domain filter, and pagination parameters.
- Verify soft-deleted company returns `404` from `GET /crm/companies/{id}`.

- **Test Data Management**: Each integration test MUST create its own test data and clean up after completion (transaction rollback or isolated test database).
- **CI/CD Integration**: Tests MUST run as part of the GitHub Actions pipeline on push and pull request to `master`.
- **Coverage Requirements**: Minimum 80% line coverage for all files under `Features/Crm/Companies/`.
- **Performance Testing**: Not required for this user story.

---

## 7. Rationale & Context

The `Company` entity is the foundational CRM record. Job applications, contacts, and outreach activities will all reference a `Company`. Establishing a clean, validated, and soft-delete-capable entity now prevents breaking changes to downstream features.

**Design decisions:**

- **UUIDv7 as PK**: Time-ordered UUIDs improve B-tree index performance vs random UUIDs and avoid integer overflow. Consistent with the existing `Book.Id` pattern using `Guid.CreateVersion7()`.
- **Soft delete (`IsActive`)**: Job hunters need historical visibility of companies they researched; hard-deletes would lose that context.
- **Case-insensitive unique index on `Name`**: Prevents duplicates such as `"Google"` vs `"google"` without normalizing the stored value.
- **Filtered unique index on `Domain`**: Allows multiple rows with `NULL` domain while enforcing uniqueness for non-null values — standard SQL Server and PostgreSQL behaviour.
- **No business logic in endpoints**: Keeps the HTTP layer thin, testable, and consistent with the existing `BookFeature` pattern.
- **Partial updates via null-coalescing**: Null fields in `UpdateCompanyRequest` mean "keep existing value" — identical to the `UpdateBook` handler pattern already in the codebase.
- **`CompanyService` as an intermediary**: Unlike the `BookFeature` which calls the repository directly from the handler, the Company feature introduces a service layer to encapsulate the uniqueness checks that require database queries before the write.

---

## 8. Dependencies & External Integrations

### External Systems
- **EXT-001**: None for this user story.

### Third-Party Services
- **SVC-001**: None for this user story.

### Infrastructure Dependencies
- **INF-001**: Relational database (SQL Server or PostgreSQL) — must support filtered/partial unique indexes for the `Domain` constraint and case-insensitive indexing for the `Name` constraint.

### Data Dependencies
- **DAT-001**: None. This is a greenfield entity with no external data imports required.

### Technology Platform Dependencies
- **PLT-001**: .NET 10 runtime — required; `Guid.CreateVersion7()` is a .NET 9+ API.
- **PLT-002**: Entity Framework Core — migrations system required for schema deployment.
- **PLT-003**: FluentValidation — for request validation, auto-discovered via assembly scanning already configured in `Program.cs`.
- **PLT-004**: Scrutor — for `IHandler<,>` decorator registration (already present in the project).

### Compliance Dependencies
- **COM-001**: None for this user story.

---

## 9. Examples & Edge Cases

### Example: Successful Create

```json
// Request
POST /crm/companies
Content-Type: application/json

{
  "name": "Acme Corporation",
  "domain": "acme.com",
  "description": "A global provider of widgets and gadgets.",
  "industry": "Manufacturing",
  "websiteUrl": "https://www.acme.com",
  "linkedInUrl": "https://www.linkedin.com/company/acme-corporation"
}

// Response 200 OK
{
  "companyId": "01954d9e-0000-7000-8000-000000000001",
  "name": "Acme Corporation",
  "domain": "acme.com",
  "description": "A global provider of widgets and gadgets.",
  "industry": "Manufacturing",
  "websiteUrl": "https://www.acme.com",
  "linkedInUrl": "https://www.linkedin.com/company/acme-corporation",
  "createdAt": "2026-02-24T12:00:00Z",
  "updatedAt": "2026-02-24T12:00:00Z",
  "isActive": true
}
```

### Example: Name Conflict

```json
// Response 409 Conflict
{
  "code": "Companies.NameConflict",
  "description": "A company named 'Acme Corporation' already exists.",
  "type": 3
}
```

### Example: Validation Error

```json
// Response 400 Bad Request
{
  "errors": [
    { "code": "NotEmptyValidator", "description": "Company name is required.", "type": 2 },
    { "code": "MinimumLengthValidator", "description": "Company name must be at least 2 characters.", "type": 2 }
  ],
  "code": "Validation.General",
  "description": "One or more validation errors occurred",
  "type": 2
}
```

### Example: Partial Update (Name only)

```json
// Request — only Name is updated; all other fields are null (keep existing)
PUT /crm/companies/01954d9e-0000-7000-8000-000000000001
{
  "name": "Acme Corp",
  "domain": null,
  "description": null,
  "industry": null,
  "websiteUrl": null,
  "linkedInUrl": null
}
// Result: Name => "Acme Corp", all other fields unchanged, UpdatedAt refreshed.
```

### Edge Cases

| Scenario | Expected Behaviour |
|---|---|
| `Name` = `"ACME CORPORATION"` when `"Acme Corporation"` exists | `409 Conflict` — `Companies.NameConflict` |
| Two companies both have `Domain = null` | Both allowed — filtered unique index only enforces non-null values |
| `WebsiteUrl` = `"not-a-url"` | `400 Bad Request` — validation error: `WebsiteUrl must be a valid absolute URL` |
| `LinkedInUrl` = `"https://linkedin.com/in/person"` (person profile) | `400 Bad Request` — `LinkedInUrl must be a LinkedIn company URL (https://www.linkedin.com/company/...)` |
| `GET /crm/companies/{id}` for soft-deleted company | `404 Not Found` — `Companies.NotFound` |
| `PUT /crm/companies/{id}` for soft-deleted company | `404 Not Found` — `Companies.NotFound` |
| `pageSize` = 101 in search request | `400 Bad Request` — validation error: `pageSize must not exceed 100` |
| `page` = 0 in search request | `400 Bad Request` — validation error: `page must be at least 1` |

---

## 10. Validation Criteria

The implementation is compliant with this specification when ALL of the following are true:

- [ ] `Companies` table exists in the database with all specified columns, column types, constraints, and indexes.
- [ ] `CompanyId` is generated as UUIDv7 (`Guid.CreateVersion7()`) at creation time.
- [ ] `CreatedAt` and `UpdatedAt` are set to UTC on creation; `UpdatedAt` is refreshed on every update and soft-delete operation.
- [ ] `POST /crm/companies` creates an active company and returns a `CompanyResponse` with `200 OK`.
- [ ] `GET /crm/companies/{id}` returns `200 OK` + `CompanyResponse` for active companies; returns `404` for non-existent or soft-deleted companies.
- [ ] `GET /crm/companies` returns a paginated `SearchCompaniesResponse` filtered by all provided query parameters; excludes soft-deleted companies.
- [ ] `PUT /crm/companies/{id}` performs partial updates — null request fields are ignored and retain their prior database values.
- [ ] `DELETE /crm/companies/{id}` sets `IsActive = false` without physically removing the database row.
- [ ] Duplicate name (case-insensitive) returns `409 Conflict` with `Companies.NameConflict`.
- [ ] Duplicate non-null domain returns `409 Conflict` with `Companies.DomainConflict`.
- [ ] Invalid `WebsiteUrl` returns `400 Bad Request` with a descriptive validation message.
- [ ] Invalid `LinkedInUrl` (not a company profile URL) returns `400 Bad Request` with a descriptive validation message.
- [ ] All handlers execute through the `ValidationDecorator` → `LoggingDecorator` → `Handler` pipeline.
- [ ] All unit tests pass.
- [ ] All integration tests pass against a real database.
- [ ] Minimum 80% line coverage for all files under `Features/Crm/Companies/`.
- [ ] OpenAPI docs expose all 5 endpoints tagged `"companies"` with correct typed response annotations.

---

## 11. Related Specifications / Further Reading

- [README — Vertical Slice Architecture](../README.md)
- [EF Core Filtered Indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes#index-filter)
- [UUIDv7 — Guid.CreateVersion7()](https://learn.microsoft.com/en-us/dotnet/api/system.guid.createversion7)
- [FluentValidation — AbstractValidator](https://docs.fluentvalidation.net/en/latest/start.html)
- Future: `spec-schema-crm-contact-entity.md` *(not yet created)*
- Future: `spec-architecture-crm-sync.md` *(not yet created — covers sync metadata, Cloudflare Tunnel, worker integration)*
