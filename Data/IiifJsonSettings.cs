using IIIF.Manifests.Serializer;
using Newtonsoft.Json;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Data;

/// <summary>
///     Shared Newtonsoft settings for round-tripping SDK node graphs through JSONB columns - used by
///     <see cref="Configurations.OwnedMappingHelpers" /> on write and <see cref="Services.ManifestGraphReader" />
///     on read, so both sides serialize/deserialize identically.
/// </summary>
internal static class IiifJsonSettings
{
    public static readonly JsonSerializerSettings Graph = new()
    {
        ContractResolver = new IIIFJsonContractResolver()
    };

    public static readonly JsonSerializerSettings Polymorphic = new()
    {
        ContractResolver = new IIIFJsonContractResolver(),
        TypeNameHandling = TypeNameHandling.Auto,
        // The SDK serializes "@id" before "$type" - MetadataPropertyHandling.Default requires $type to
        // appear first, so it fails resolving the concrete type unless it's told to scan ahead for it.
        MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
    };
}
