using IIIF.POC.PostgreSqlRelationalV3Store.Domain;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Pages.Manifests;

public sealed class DeleteModel(ManifestStoreService store) : PageModel
{
    public ManifestEntity Manifest { get; private set; } = default!;
    public string Label => ManifestRelationalMapper.FirstLabel(Manifest.Label);
    [BindProperty] public uint Version { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await store.FindAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        Manifest = entity; Version = entity.Version; return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await store.DeleteAsync(id, Version, cancellationToken);
        if (!result.Succeeded)
        {
            var entity = await store.FindAsync(id, cancellationToken);
            if (entity is null) return RedirectToPage("Index");
            Manifest = entity;
            ModelState.AddModelError(string.Empty, result.Error ?? "The Manifest could not be deleted.");
            return Page();
        }
        TempData["StatusMessage"] = "Manifest aggregate and all owned rows were deleted.";
        return RedirectToPage("Index");
    }
}
