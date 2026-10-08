# IIIF.POC.PostgreSqlRelationalV3Store

A proof-of-concept ASP.NET Core Razor Pages application that stores a normalized **IIIF Presentation API 3** Manifest as a relational PostgreSQL aggregate.

Known Presentation 3 elements are mapped with EF Core `OwnsOne` and `OwnsMany`. Unknown and extension properties round-trip through PostgreSQL `jsonb` columns instead of being dropped or forced into a relational shape.

[![Architecture diagram](https://gitdiagram.com/diagram-badge.svg)](https://gitdiagram.com/kiarashminoo/IIIF.POC.PostgreSqlRelationalV3Store?utm_source=readme&utm_medium=badge)

Core SDK:

https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net

## Stack

- .NET 10
- ASP.NET Core Razor Pages
- Entity Framework Core 10
- Npgsql EF Core provider 10
- PostgreSQL 18
- IIIF Manifest Serializer for .NET 3.0.17

## How it works

```text
Presentation 2.x or 3.0 JSON
        ↓
SDK version detection
        ↓
SDK validation
        ↓
Normalization to Presentation 3
        ↓
EF Core owned aggregate (ManifestEntity → Manifest)
        ↓
PostgreSQL relational tables, extension data in jsonb columns
```

Known Presentation 3 elements — labels, summaries, metadata, behavior, homepage/thumbnail/rendering/seeAlso/partOf links, providers, and Ranges (with their own nested labels/metadata/providers) — are mapped relationally with EF Core `OwnsOne`/`OwnsMany`. Canvas ordering (`Manifest.Items`), Range items, `start`, and `placeholderCanvas`/`accompanyingCanvas` round-trip through targeted `jsonb` columns instead, since the SDK exposes them only as polymorphic or fully-nested graphs with no fixed relational shape. CRUD covers create (validate → normalize → insert), read, update (replace-in-transaction with PostgreSQL `xmin` optimistic concurrency), delete (cascading), and export back to Presentation 2.0, 2.1, or 3.0.

## Documentation

The full mapping design — the owned-type tree, the JSONB conversion rules, and the CRUD/export flows — is under [`/docs`](docs/README.md), one README per folder with its types, members, and diagrams.

- [Data](docs/Data/README.md) `Types:2` `Files:2` `Diagrams:✓`
  - [Configurations](docs/Data/Configurations/README.md) `Types:2` `Files:2` `Diagrams:✓`
- [Domain](docs/Domain/README.md) `Types:1` `Files:1` `Diagrams:✓`
- [Migrations](docs/Migrations/README.md) `Types:2` `Files:3` `Diagrams:✓`
- [Models](docs/Models/README.md) `Types:5` `Files:5` `Diagrams:✓`
- [Pages](docs/Pages/README.md) `Types:2` `Files:6` `Diagrams:✗`
  - [Manifests](docs/Pages/Manifests/README.md) `Types:5` `Files:10` `Diagrams:✓`
  - [Shared](docs/Pages/Shared/README.md) `Types:0` `Files:2` `Diagrams:✗`
- [Services](docs/Services/README.md) `Types:2` `Files:2` `Diagrams:✓`
- [wwwroot](docs/wwwroot/README.md) `Types:0` `Files:1` `Diagrams:✗`

## Package Dependencies

| Package | Version | Description | Links |
|---|---|---|---|
| `IIIF.Manifest.Serializer.Net` | 3.0.17 | IIIF Presentation parsing, validation, normalization, and serialization | [NuGet](https://www.nuget.org/packages/IIIF.Manifest.Serializer.Net) · [GitHub](https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net) |
| `Microsoft.EntityFrameworkCore` | 10.0.11 | EF Core runtime | [NuGet](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore) |
| `Microsoft.EntityFrameworkCore.Design` | 10.0.11 | Design-time tooling for migrations | [NuGet](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.Design) |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | 10.0.3 | PostgreSQL provider for EF Core | [NuGet](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL) |

No `NuGet.Config` is present, so restore uses the default feed (`https://api.nuget.org/v3/index.json`).

## Build

`dotnet restore` / `dotnet build -c Release`

## Run

Start PostgreSQL:

```bash
docker compose up -d
```

Restore and run:

```bash
dotnet restore
dotnet run
```

`appsettings.json` ships with:

```text
Host=localhost;Port=5432;Database=iiif_relational_v3;Username=postgres;Password=123456
```

This does not match the `iiif`/`iiif_dev_password` credentials `compose.yaml` provisions — update one side or override the connection string with the `ConnectionStrings__PostgreSql` environment variable before running against the compose container.

## POC initialization

The POC uses:

```csharp
await db.Database.EnsureCreatedAsync();
```

A migration is also committed under `Migrations/` for environments that prefer `dotnet ef database update` — the running POC does not apply it automatically.

## Production considerations

Before production use, add:

- EF Core migrations and migration tests;
- integration tests against PostgreSQL;
- authentication and resource-level authorization;
- pagination and query-specific indexes;
- request and JSON size limits;
- revision or audit history;
- soft deletion or retention rules where required;
- structured logging, tracing, health checks, and metrics;
- secret management and restricted database roles;
- backup and restore procedures;
- explicit handling for unsupported future Presentation properties;
- performance tests for large Manifests and deep owned graphs.

## Important trade-off

This design makes known Manifest elements relational and queryable, but it creates more tables, joins, and migration coupling than storing a complete Manifest document in one JSONB column.

It is most appropriate when the application needs relational queries and constraints across the IIIF graph.

For a repository that mainly stores and serves opaque Manifests, complete-document JSONB storage may remain simpler.

## License

Free to use — this is a proof-of-concept repository with no usage restrictions.
