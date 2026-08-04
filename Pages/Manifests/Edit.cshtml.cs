using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Pages.Manifests;

public sealed class EditModel(ManifestStoreService store) : PageModel
{
    [BindProperty]
    public ManifestInputModel Input { get; set; } = new();
    public Guid Id { get; private set; }
    public IReadOnlyList<ManifestFinding> Findings { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await store.FindAsync(id, cancellationToken);
        if (entity is null) return NotFound();
        Id = id;
        Input.Json = ManifestRelationalMapper.ToCanonicalJson(entity, indented: true);
        Input.Version = entity.Version;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id, CancellationToken cancellationToken)
    {
        Id = id;
        if (!ModelState.IsValid) return Page();
        var result = await store.UpdateAsync(id, Input.Version, Input.Json, cancellationToken);
        Findings = result.Findings;
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "The Manifest could not be updated.");
            return Page();
        }
        TempData["StatusMessage"] = "Relational Manifest aggregate updated in PostgreSQL.";
        return RedirectToPage("Details", new { id });
    }
}
