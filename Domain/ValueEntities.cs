namespace IIIF.POC.PostgreSqlRelationalV3Store.Domain;

public sealed class LanguageMapEntity
{
    public List<LanguageValueEntity> Values { get; set; } = [];
}

public sealed class LanguageValueEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string Language { get; set; } = "none";
    public string Value { get; set; } = "";
}

public sealed class MetadataEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public LanguageMapEntity Label { get; set; } = new();
    public LanguageMapEntity Value { get; set; } = new();
}

public sealed class RequiredStatementEntity
{
    public LanguageMapEntity Label { get; set; } = new();
    public LanguageMapEntity Value { get; set; } = new();
}

public sealed class OrderedStringEntity
{
    public Guid RowId { get; set; } = Guid.NewGuid();
    public int Position { get; set; }
    public string Value { get; set; } = "";
}
