namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class CanvasEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string IiifId { get; set; } = "";
    public string Type { get; set; } = "Canvas";
    public int? Height { get; set; }
    public int? Width { get; set; }
    public double? Duration { get; set; }
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
    public List<AnnotationPageEntity> Items { get; set; } = [];
    public List<AnnotationPageEntity> Annotations { get; set; } = [];
    public EmbeddedCanvasEntity? PlaceholderCanvas { get; set; }
    public EmbeddedCanvasEntity? AccompanyingCanvas { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class EmbeddedCanvasEntity
{
    public string IiifId { get; set; } = "";
    public string Type { get; set; } = "Canvas";
    public int? Height { get; set; }
    public int? Width { get; set; }
    public double? Duration { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public List<AnnotationPageEntity> Items { get; set; } = [];
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class AnnotationPageEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string IiifId { get; set; } = "";
    public string Type { get; set; } = "AnnotationPage";
    public List<AnnotationEntity> Items { get; set; } = [];
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class AnnotationEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string IiifId { get; set; } = "";
    public string Type { get; set; } = "Annotation";
    public LanguageMapEntity Label { get; set; } = new();
    public List<OrderedStringEntity> Motivation { get; set; } = [];
    public List<ContentResourceEntity> Bodies { get; set; } = [];
    public List<TargetEntity> Targets { get; set; } = [];
    public string? TimeMode { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}
