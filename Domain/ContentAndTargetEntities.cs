namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class ContentResourceEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Id { get; set; }
    public string Type { get; set; } = "Image";
    public string? Format { get; set; }
    public string? Profile { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
    public double? Duration { get; set; }
    public string? Value { get; set; }
    public string? TextDirection { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public List<OrderedStringEntity> Language { get; set; } = [];
    public List<OrderedStringEntity> Behavior { get; set; } = [];
    public List<ServiceEntity> Services { get; set; } = [];
    public List<AnnotationPageEntity> Annotations { get; set; } = [];
    public List<ChoiceItemEntity> Items { get; set; } = [];
    public SpecificResourceEntity? SpecificResource { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class ChoiceItemEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Id { get; set; }
    public string Type { get; set; } = "Image";
    public string? Format { get; set; }
    public string? Profile { get; set; }
    public int? Height { get; set; }
    public int? Width { get; set; }
    public double? Duration { get; set; }
    public string? Value { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public List<OrderedStringEntity> Language { get; set; } = [];
    public List<ServiceEntity> Services { get; set; } = [];
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class SpecificResourceEntity
{
    public string Type { get; set; } = "SpecificResource";
    public string? SourceId { get; set; }
    public string? SourceType { get; set; }
    public SelectorEntity? Selector { get; set; }
    public string? StyleClass { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class TargetEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? SourceId { get; set; }
    public string? SourceType { get; set; }
    public SelectorEntity? Selector { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}

public sealed class SelectorEntity
{
    public string? Id { get; set; }
    public string Type { get; set; } = "FragmentSelector";
    public string? Value { get; set; }
    public string? ConformsTo { get; set; }
    public int? X { get; set; }
    public int? Y { get; set; }
    public int? T { get; set; }
    public string AdditionalPropertiesJson { get; set; } = "{}";
}
