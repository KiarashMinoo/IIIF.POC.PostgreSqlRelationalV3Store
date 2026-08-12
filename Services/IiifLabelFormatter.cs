using IIIF.Manifests.Serializer.Properties;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Services;

public static class IiifLabelFormatter
{
    public static string FirstOrDefault(IReadOnlyCollection<Label> label) =>
        label.Select(x => x.Value).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Untitled Manifest";
}
