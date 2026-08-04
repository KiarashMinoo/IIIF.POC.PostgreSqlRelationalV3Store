namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class LinkEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Format { get; set; }
    public string? Profile { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
    public double? Duration { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public List<OrderedStringEntity> Language { get; set; } = [];
    public List<ServiceEntity> Services { get; set; } = [];
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class ServiceEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Context { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? Profile { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class AgentEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Id { get; set; }
    public string Type { get; set; } = "Agent";
    public LanguageMapEntity Label { get; set; } = new();
    public List<LinkEntity> Homepage { get; set; } = [];
    public List<LinkEntity> Logo { get; set; } = [];
    public List<LinkEntity> SeeAlso { get; set; } = [];
    public List<ServiceEntity> Services { get; set; } = [];
    public string AdditionalPropertiesJson { get; set; } = "{}";
}
