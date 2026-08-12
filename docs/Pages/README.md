# Pages

## Contents

- [Overview](#overview)
- [Files](#files)
- [Types](#types)
  - [IndexModel](#indexmodel)
  - [ErrorModel](#errormodel)
- [See also](#see-also)

## Overview

The Razor Pages shell: the site root (which just redirects into the app) and the shared error page. The actual application lives one level down in [Pages/Manifests](Manifests/README.md); shared layout and partials are in [Pages/Shared](Shared/README.md).

## Files

| File | Primary type(s) | Responsibility |
|---|---|---|
| `Index.cshtml` / `Index.cshtml.cs` | `IndexModel` | `/` — redirects to `/Manifests` |
| `Error.cshtml` / `Error.cshtml.cs` | `ErrorModel` | The `UseExceptionHandler("/Error")` target |
| `_ViewImports.cshtml` | — | Shared `@using`s, `@namespace`, and tag helper registration for every page |
| `_ViewStart.cshtml` | — | Sets `_Layout` as the default layout for every page |

## Types

### IndexModel

- **Namespace:** `IIIF.POC.PostgreSqlRelationalV3Store.Pages`
- `OnGet() : IActionResult` — always `RedirectToPage("/Manifests/Index")`. There's no standalone home page; the manifest list *is* the home page.

### ErrorModel

- **Namespace:** `IIIF.POC.PostgreSqlRelationalV3Store.Pages`
- `[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]` — never cached.
- `RequestId` / `ShowRequestId` — populated from `Activity.Current?.Id` or the ASP.NET Core trace identifier, so an error page can be correlated back to server logs.

## See also

- [Pages/Manifests](Manifests/README.md) — the actual CRUD pages
- [Pages/Shared](Shared/README.md) — the layout `_ViewStart.cshtml` selects

[↑ Back to top](#contents)
