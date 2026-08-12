using IIIF.Manifests.Serializer.Nodes;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Models;

public sealed record ManifestDetail(
    Guid Id,
    string IiifId,
    string SourceVersion,
    string ContentHash,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    uint Version,
    Manifest Node);
