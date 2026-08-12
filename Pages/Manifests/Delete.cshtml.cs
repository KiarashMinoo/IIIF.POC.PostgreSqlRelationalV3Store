using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Pages.Manifests;

public sealed class DeleteModel(ManifestStoreService store) : PageModel
{
    public ManifestDetail Manifest { get; private set; } = default!;
    public string Label => IiifLabelFormatter.FirstOrDefault(Manifest.Node.Label);
    [BindProperty] public uint Version { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var detail = await store.FindAsync(id, cancellationToken);
        if (detail is null) return NotFound();
        Manifest = detail; Version = detail.Version; return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await store.DeleteAsync(id, Version, cancellationToken);
        if (!result.Succeeded)
        {
            var detail = await store.FindAsync(id, cancellationToken);
            if (detail is null) return RedirectToPage("Index");
            Manifest = detail;
            ModelState.AddModelError(string.Empty, result.Error ?? "The Manifest could not be deleted.");
            return Page();
        }
        TempData["StatusMessage"] = "Manifest deleted from PostgreSQL.";
        return RedirectToPage("Index");
    }
}
