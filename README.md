# IIIF.POC.PostgreSqlRelationalV3Store

A proof-of-concept ASP.NET Core Razor Pages application that stores a normalized **IIIF Presentation API 3** Manifest as a relational PostgreSQL aggregate.

Known Presentation 3 elements are mapped with EF Core `OwnsOne` and `OwnsMany`. Only unknown and extension properties are stored in PostgreSQL `jsonb` columns named `additional_properties`.

Core SDK:

https://github.com/KiarashMinoo/IIIF.Manifest.Serializer.Net

## Recommended repository name

```text
IIIF.POC.PostgreSqlRelationalV3Store
```

## Recommended GitHub About description

**Proof-of-concept Razor Pages app that normalizes IIIF Manifests to Presentation 3 and stores the complete known object graph relationally with EF Core OwnsOne/OwnsMany, PostgreSQL, CRUD, and JSONB extension data.**

## Suggested topics

```text
iiif
dotnet
csharp
aspnet-core
razor-pages
postgresql
entity-framework-core
npgsql
owned-entities
relational-modeling
jsonb
crud
optimistic-concurrency
digital-libraries
proof-of-concept
nuget
```

## Stack

- .NET 10
- ASP.NET Core Razor Pages
- Entity Framework Core 10
- Npgsql EF Core provider 10
- PostgreSQL 18
- IIIF Manifest Serializer for .NET 3.0.13

## Persistence boundary

```text
Presentation 2.x or 3.0 JSON
        ↓
SDK version detection
        ↓
SDK validation
        ↓
Normalization to Presentation 3
        ↓
JSON-to-persistence projection
        ↓
EF Core owned aggregate
        ↓
PostgreSQL relational tables
```

The SDK model is not used directly as the EF Core entity model. A separate persistence projection mirrors the Presentation 3 shape. This avoids adding persistence concerns to the SDK while retaining its version-aware parsing and serialization behavior.

## Relationally mapped elements

The POC maps known Presentation 3 structures with `OwnsOne` or `OwnsMany`, including:

- Manifest
- language maps and language values
- metadata
- required statement
- behavior values
- homepage, thumbnail, rendering, seeAlso, and partOf links
- providers/agents
- services
- Canvases
- AnnotationPages
- Annotations
- annotation bodies and targets
- Image, Audio, Video, TextualBody, Choice, and SpecificResource-style body data
- selectors
- Ranges and Range items
- annotations on supported resources
- placeholder and accompanying Canvases
- start

Every owned collection has an explicit surrogate row key and a `position` column so IIIF array order is preserved.

## JSONB usage

The POC does **not** store the whole Manifest as JSONB.

Each resource-like persistence entity has:

```csharp
public string AdditionalPropertiesJson { get; set; } = "{}";
```

It is mapped as:

```csharp
builder.Property(x => x.AdditionalPropertiesJson)
    .HasColumnName("additional_properties")
    .HasColumnType("jsonb");
```

This field contains only properties not explicitly represented by the relational Presentation 3 model, such as:

- extension properties such as `navPlace`;
- Image API service-specific fields such as `tiles`;
- future or unknown properties;
- third-party extension data.

Known Presentation 3 properties are always stored relationally.

## Entity configuration

The central mapping is:

```text
Data/Configurations/ManifestEntityConfiguration.cs
```

Reusable owned-type mapping helpers are in:

```text
Data/Configurations/OwnedMappingHelpers.cs
```

Examples:

```csharp
builder.OwnsOne(x => x.Label, label =>
    OwnedMappingHelpers.LanguageMap(
        label,
        "m_label_values",
        "manifest_id"));

builder.OwnsMany(x => x.Items, canvases =>
    ConfigureCanvas(
        canvases,
        "canvases",
        "manifest_id",
        "c"));
```

`OwnsMany` rows use explicit `Guid` keys plus parent foreign keys. This keeps each row identifiable while preserving aggregate ownership and cascade deletion.

## CRUD

- **Create:** paste Presentation 2.x or 3.0 JSON, validate, normalize, project, and insert the owned aggregate.
- **Read:** query Manifest fields and relational label values; display counts without loading raw JSON documents.
- **Update:** rebuild and replace the owned aggregate with PostgreSQL `xmin` optimistic concurrency.
- **Delete:** delete the root and cascade through all owned rows.
- **Export:** reconstruct Presentation 3 JSON from relational rows, then use the SDK to export 3.0, 2.1, or 2.0.

## Additional-property round trip

The mapper uses this rule:

```text
known property
    → relational column/table

unknown or extension property
    → nearest resource's additional_properties jsonb
```

When exporting, the JSONB object is loaded first and known relational properties are written over it. This prevents extension data from overriding the canonical known values.

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

Default connection string:

```text
Host=localhost;Port=5432;Database=iiif_relational_v3;Username=iiif;Password=iiif_dev_password
```

Override it with:

```text
ConnectionStrings__PostgreSql
```

## POC initialization

The POC uses:

```csharp
await db.Database.EnsureCreatedAsync();
```

Use EF Core migrations before production deployment.

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

## Recursive annotation boundary

Presentation resources can theoretically contain recursively nested annotation graphs. A finite static `OwnsOne`/`OwnsMany` model cannot represent unlimited recursive ownership without an explicit boundary.

This POC relationally maps one level of annotations attached to content resources. If a body inside that level contains another `annotations` property, the mapper rejects the document rather than silently moving that known property into JSONB.

A production implementation that requires unlimited recursive annotation depth should use regular self-referencing entities for that portion of the graph, or define an explicit supported profile/depth.

## Important trade-off

This design makes known Manifest elements relational and queryable, but it creates more tables, joins, and migration coupling than storing a complete Manifest document in one JSONB column.

It is most appropriate when the application genuinely needs relational queries and constraints across the IIIF graph.

For a repository that mainly stores and serves opaque Manifests, complete-document JSONB storage may remain simpler.

## License

Add the license appropriate for the POC repository before publishing.
