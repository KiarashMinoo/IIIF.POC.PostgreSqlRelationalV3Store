using System.Text;
using IIIF.Manifests.Serializer;
using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Pages.Manifests;

public sealed class DetailsModel(ManifestStoreService store) : PageModel
{
    public ManifestDetail Manifest { get; private set; } = default!;
    public string Json { get; private set; } = "";
    public string Label => IiifLabelFormatter.FirstOrDefault(Manifest.Node.Label);

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await store.FindAsync(id, cancellationToken);
        if (detail is null) return NotFound();
        Manifest = detail;
        Json = await store.CanonicalJsonAsync(id, cancellationToken) ?? "";
        return Page();
    }

    public async Task<IActionResult> OnGetExportAsync(Guid id, string target, CancellationToken cancellationToken)
    {
        var version = target.ToLowerInvariant() switch
        {
            "v2.0" => IiifPresentationVersion.V2_0,
            "v2.1" => IiifPresentationVersion.V2_1,
            _ => IiifPresentationVersion.V3_0
        };
        var json = await store.ExportAsync(id, version, cancellationToken);
        if (json is null) return NotFound();
        return File(Encoding.UTF8.GetBytes(json), "application/json", $"iiif-manifest-{id:N}-{target}.json");
    }
}
