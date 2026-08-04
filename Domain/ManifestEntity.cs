namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class ManifestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string IiifId { get; set; } = "";
    public string Type { get; set; } = "Manifest";
    public string Context { get; set; } = "http://iiif.io/api/presentation/3/context.json";
    public string SourceVersion { get; set; } = "";
    public string? Rights { get; set; }
    public DateTimeOffset? NavDate { get; set; }
    public string? ViewingDirection { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public LanguageMapEntity Summary { get; set; } = new();
    public List<MetadataEntity> Metadata { get; set; } = [];
    public RequiredStatementEntity? RequiredStatement { get; set; }
    public List<OrderedStringEntity> Behavior { get; set; } = [];
    public List<LinkEntity> Homepage { get; set; } = [];
    public List<LinkEntity> Thumbnail { get; set; } = [];
    public List<LinkEntity> Rendering { get; set; } = [];
    public List<LinkEntity> SeeAlso { get; set; } = [];
    public List<LinkEntity> PartOf { get; set; } = [];
    public List<AgentEntity> Provider { get; set; } = [];
    public List<ServiceEntity> Services { get; set; } = [];
    public List<CanvasEntity> Items { get; set; } = [];
    public List<RangeEntity> Structures { get; set; } = [];
    public List<AnnotationPageEntity> Annotations { get; set; } = [];
    public EmbeddedCanvasEntity? PlaceholderCanvas { get; set; }
    public EmbeddedCanvasEntity? AccompanyingCanvas { get; set; }
    public StartEntity? Start { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
    public string ContentHash { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public uint Version { get; private set; }
}
