using IIIF.Manifests.Serializer.Nodes;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class ManifestEntity
{
    public const string IiifId = nameof(IiifId);
    public const string Label = nameof(Label);
    public const string SourceVersion = nameof(SourceVersion);
    public const string ContentHash = nameof(ContentHash);
    public const string CanvasCount = nameof(CanvasCount);
    public const string StructureCount = nameof(StructureCount);
    public const string CreatedAtUtc = nameof(CreatedAtUtc);
    public const string UpdatedAtUtc = nameof(UpdatedAtUtc);
    public const string Version = nameof(Version);

    public Guid Id { get; set; } = Guid.NewGuid();
    public Manifest Manifest { get; set; } = null!;
}