namespace IIIF.POC.PostgreSqlRelationalV3Store.Models;

public sealed class ManifestOperationResult
{
    public bool Succeeded { get; init; }
    public bool ConcurrencyConflict { get; init; }
    public Guid? Id { get; init; }
    public string? Error { get; init; }
    public IReadOnlyList<ManifestFinding> Findings { get; init; } = [];
}