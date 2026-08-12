using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Pages.Manifests;

public sealed class CreateModel(ManifestStoreService store) : PageModel
{
    [BindProperty]
    public ManifestInputModel Input { get; set; } = new();

    public IReadOnlyList<ManifestFinding> Findings { get; private set; } = [];

    public void OnGet() => Input.Json = SampleManifest;

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Page();
        var result = await store.CreateAsync(Input.Json, cancellationToken);
        Findings = result.Findings;
        if (!result.Succeeded || result.Id is null)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The Manifest could not be stored.");
            return Page();
        }

        TempData["StatusMessage"] = "Manifest validated, normalized to Presentation 3, and stored as JSONB in PostgreSQL.";
        return RedirectToPage("Details", new { id = result.Id.Value });
    }

    private const string SampleManifest = """
{
  "@context": "http://iiif.io/api/presentation/3/context.json",
  "id": "https://example.org/iiif/relational-v3/manifest",
  "type": "Manifest",
  "label": { "en": ["Relational Presentation 3 Demo"] },
  "summary": { "en": ["Known IIIF properties are stored relationally."] },
  "metadata": [
    {
      "label": { "en": ["Repository"] },
      "value": { "en": ["PostgreSQL relational POC"] }
    }
  ],
  "rights": "https://creativecommons.org/licenses/by/4.0/",
  "items": [
    {
      "id": "https://example.org/iiif/relational-v3/canvas/1",
      "type": "Canvas",
      "label": { "en": ["Page 1"] },
      "height": 1200,
      "width": 900,
      "items": [
        {
          "id": "https://example.org/iiif/relational-v3/page/1",
          "type": "AnnotationPage",
          "items": [
            {
              "id": "https://example.org/iiif/relational-v3/annotation/1",
              "type": "Annotation",
              "motivation": "painting",
              "body": {
                "id": "https://example.org/image/full/max/0/default.jpg",
                "type": "Image",
                "format": "image/jpeg",
                "height": 1200,
                "width": 900,
                "service": [
                  {
                    "id": "https://example.org/image",
                    "type": "ImageService3",
                    "profile": "level1",
                    "tiles": [{ "width": 512, "scaleFactors": [1, 2, 4] }]
                  }
                ]
              },
              "target": "https://example.org/iiif/relational-v3/canvas/1"
            }
          ]
        }
      ]
    }
  ],
  "navPlace": {
    "type": "FeatureCollection",
    "features": []
  }
}
""";
}
