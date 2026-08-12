# Documentation

Generated reference documentation for `IIIF.POC.PostgreSqlRelationalV3Store`, a proof-of-concept Razor Pages app that normalizes IIIF Presentation Manifests to Presentation 3 and stores them as an EF Core owned aggregate over PostgreSQL. Start at the project [`/README.md`](../README.md) for the overview, stack, and run instructions; the pages below cover each folder's types and how they fit together.

## Areas

- [Data](Data/README.md) — the `DbContext`, the Newtonsoft settings shared by the JSONB conversions, and:
  - [Configurations](Data/Configurations/README.md) — the EF Core owned-type mapping for the SDK's `Manifest`/`Structure` node graph
- [Domain](Domain/README.md) — `ManifestEntity`, the aggregate root
- [Migrations](Migrations/README.md) — the committed EF Core migration and model snapshot
- [Models](Models/README.md) — page-facing DTOs and the CRUD result/finding types
- [Pages](Pages/README.md) — the Razor Pages shell (home redirect, error page), and:
  - [Manifests](Pages/Manifests/README.md) — the CRUD pages (list, create, details, edit, delete)
  - [Shared](Pages/Shared/README.md) — the layout and the validation-findings partial
- [Services](Services/README.md) — `ManifestStoreService` (validate → normalize → persist → export) and the label-formatting helper it uses
- [wwwroot](wwwroot/README.md) — the static stylesheet
