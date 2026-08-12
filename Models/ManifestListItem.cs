namespace IIIF.POC.PostgreSqlRelationalV3Store.Models;

public sealed record ManifestListItem(
    Guid Id,
    string IiifId,
    string Label,
    string SourceVersion,
    int CanvasCount,
    int StructureCount,
    DateTimeOffset UpdatedAtUtc,
    uint Version);