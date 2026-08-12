# Pages / Shared

## Contents

- [Overview](#overview)
- [Files](#files)
- [See also](#see-also)

## Overview

The layout every page renders inside, and the one partial shared by the pages that submit a Manifest for validation. No C# types live in this folder — both files are Razor templates.

## Files

| File | Responsibility |
|---|---|
| `_Layout.cshtml` | Site chrome: header/nav, the `TempData["StatusMessage"]` banner, `@RenderBody()`, footer |
| `_Findings.cshtml` | Renders a `IReadOnlyList<ManifestFinding>` as a severity/rule/path/message table; renders nothing when the list is empty |

### _Layout.cshtml

Selected for every page by [`Pages/_ViewStart.cshtml`](../README.md#files). Links to `/Manifests` (list) and `/Manifests/Create` (add) live in the nav; a `TempData`-driven status banner surfaces the one-line confirmation messages set by [Create/Edit/Delete](../Manifests/README.md) after a successful mutation.

### _Findings.cshtml

`@model IReadOnlyList<ManifestFinding>` — used by [`Create.cshtml`](../Manifests/README.md#createmodel) and [`Edit.cshtml`](../Manifests/README.md#editmodel) to show the SDK validator's findings (severity badge, rule id, JSON path, message) regardless of whether the submission ultimately succeeded, since a valid Manifest can still carry non-blocking findings.

## See also

- [Pages](../README.md) — `_ViewStart.cshtml`, which selects `_Layout`
- [Pages/Manifests](../Manifests/README.md) — the pages that use `_Findings`
- [Models](../../Models/README.md) — `ManifestFinding`

[↑ Back to top](#contents)
