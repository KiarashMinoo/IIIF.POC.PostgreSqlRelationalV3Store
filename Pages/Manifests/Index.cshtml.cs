using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Pages.Manifests;

public sealed class IndexModel(ManifestStoreService store) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public IReadOnlyList<ManifestListItem> Manifests { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken cancellationToken) =>
        Manifests = await store.ListAsync(Query, cancellationToken);
}
