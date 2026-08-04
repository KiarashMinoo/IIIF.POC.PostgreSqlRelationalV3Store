namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class RangeEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string IiifId { get; set; } = "";
    public string Type { get; set; } = "Range";
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
    public List<RangeItemEntity> Items { get; set; } = [];
    public List<AnnotationPageEntity> Annotations { get; set; } = [];
    public LinkEntity? Supplementary { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class RangeItemEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Id { get; set; }
    public string Type { get; set; } = "Canvas";
    public string? SourceId { get; set; }
    public string? SourceType { get; set; }
    public SelectorEntity? Selector { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class StartEntity
{
    public string? Id { get; set; }
    public string Type { get; set; } = "Canvas";
    public string? SourceId { get; set; }
    public string? SourceType { get; set; }
    public SelectorEntity? Selector { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}
