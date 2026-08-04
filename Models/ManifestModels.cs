using System.ComponentModel.DataAnnotations;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Models;

public sealed class ManifestInputModel
{
    [Required]
    [Display(Name = "Manifest JSON")]
    public string Json { get; set; } = "";

    public uint Version { get; set; }
}

public sealed record ManifestFinding(string RuleId, string Severity, string Path, string Message);

public sealed class ManifestOperationResult
{
    public bool Succeeded { get; init; }
    public bool ConcurrencyConflict { get; init; }
    public Guid? Id { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<ManifestFinding> Findings { get; init; } = [];
}

public sealed record ManifestListItem(
    Guid Id,
    string IiifId,
    string Label,
    string SourceVersion,
    int CanvasCount,
    int StructureCount,
    DateTimeOffset UpdatedAtUtc,
    uint Version);
