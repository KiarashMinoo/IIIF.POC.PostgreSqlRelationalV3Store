using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Properties;
using IIIF.Manifests.Serializer.Properties.Interfaces;
using IIIF.Manifests.Serializer.Properties.MetadataProperty;
using IIIF.Manifests.Serializer.Properties.MetadataProperty.MetadataValue;
using IIIF.Manifests.Serializer.Shared;
using IIIF.Manifests.Serializer.Shared.ValuableItem;
using IIIF.POC.PostgreSqlRelationalV3Store.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Data.Configurations;

internal static class OwnedMappingHelpers
{
    private static readonly ValueComparer<IReadOnlyCollection<string>> StringCollectionComparer = new(
        (a, b) => (a ?? Array.Empty<string>()).SequenceEqual(b ?? Array.Empty<string>()),
        collection => collection.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
        collection => collection.ToList());

    private static readonly ValueComparer<IReadOnlyCollection<IBaseItem>> BaseItemCollectionComparer = new(
        (a, b) => (a ?? Array.Empty<IBaseItem>()).SequenceEqual(b ?? Array.Empty<IBaseItem>()),
        collection => collection.Aggregate(0, (hash, item) => HashCode.Combine(hash, item)),
        collection => collection.ToList());

    private static PropertyBuilder<IReadOnlyCollection<string>> ContextMapping(
        PropertyBuilder<IReadOnlyCollection<string>> property)
    {
        property
            .HasColumnName("context")
            .HasMaxLength(2048)
            .IsRequired()
            .HasConversion(
                context => string.Join(" ", context),
                context => context.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        property.Metadata.SetValueComparer(StringCollectionComparer);

        return property;
    }

    //Generic elements

    public static OwnedNavigationBuilder<TEntity, TRelatedEntity> BaseItemMapping<TEntity, TRelatedEntity>(
        OwnedNavigationBuilder<TEntity, TRelatedEntity> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
        where TRelatedEntity : BaseItem<TRelatedEntity>
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Ignore(x => x.HasChanges);
        builder.Ignore(x => x.Service);
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id").HasMaxLength(2048).IsRequired().ValueGeneratedNever();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100);
        ContextMapping(builder.Property(x => x.Context));

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, TRelatedEntity> FormattableItemMapping<TEntity, TRelatedEntity>(
        OwnedNavigationBuilder<TEntity, TRelatedEntity> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
        where TRelatedEntity : FormattableItem<TRelatedEntity>
    {
        BaseItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Format).HasColumnName("format").HasMaxLength(255);

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, TRelatedEntity> DimensionedFormattableItemMapping<TEntity, TRelatedEntity>(
        OwnedNavigationBuilder<TEntity, TRelatedEntity> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
        where TRelatedEntity : FormattableItem<TRelatedEntity>, IDimensionSupport<TRelatedEntity>
    {
        FormattableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");

        return builder;
    }

    /// <summary>
    ///     Maps a <see cref="ValuableItem{TValuableItem}" />-derived value (a bare/optionally
    ///     language-tagged value with no natural id) via a surrogate row key, since <see cref="ValuableItem{T}.Value" />
    ///     and any accompanying language tag can both be null/repeat and so cannot safely form a
    ///     not-null primary key on their own.
    /// </summary>
    public static OwnedNavigationBuilder<TEntity, TRelatedEntity> ValuableItemMapping<TEntity, TRelatedEntity>(
        OwnedNavigationBuilder<TEntity, TRelatedEntity> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
        where TRelatedEntity : ValuableItem<TRelatedEntity>
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Ignore(x => x.HasChanges);
        builder.Property<Guid>("RowId").HasColumnName("row_id").ValueGeneratedOnAdd();
        builder.HasKey("RowId");

        builder.Property(x => x.Value).HasColumnName("value").HasMaxLength(255).IsRequired();

        return builder;
    }

    //Elements

    public static OwnedNavigationBuilder<TEntity, Label> LabelMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, Label> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        ValuableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Language).HasColumnName("language").HasMaxLength(35);

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, Description> SummaryMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, Description> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        ValuableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Language).HasColumnName("language").HasMaxLength(35);

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, Behavior> BehaviorMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, Behavior> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        ValuableItemMapping(builder, table, ownerForeignKey);

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, MetadataValue> MetadataValueMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, MetadataValue> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        ValuableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Language).HasColumnName("language").HasMaxLength(35);

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, Homepage> HomepageMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, Homepage> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        FormattableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Label).HasColumnName("label");

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, Rendering> RenderingMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, Rendering> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        FormattableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Label).HasColumnName("label").IsRequired();

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, SeeAlso> SeeAlsoMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, SeeAlso> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        FormattableItemMapping(builder, table, ownerForeignKey);

        builder.Property(x => x.Profile).HasColumnName("profile").HasMaxLength(2048);
        builder.Property(x => x.Label).HasColumnName("label");

        return builder;
    }

    public static OwnedNavigationBuilder<TEntity, Provider> ProviderMapping<TEntity>(
        OwnedNavigationBuilder<TEntity, Provider> builder,
        string table,
        string ownerForeignKey)
        where TEntity : class
    {
        FormattableItemMapping(builder, table, ownerForeignKey);

        // Three ownership levels deep (ManifestEntity -> Manifest -> Provider -> these), EF cannot infer the
        // shadow FK's type from Provider's own (string) key on its own - it must be declared explicitly first.
        const string providerForeignKey = "provider_id";
        builder.OwnsMany(x => x.Label, label =>
        {
            label.Property<string>(providerForeignKey);
            LabelMapping(label, $"{table}_labels", providerForeignKey);
        });
        builder.OwnsMany(x => x.Homepage, links =>
        {
            links.Property<string>(providerForeignKey);
            HomepageMapping(links, $"{table}_homepages", providerForeignKey);
        });
        builder.OwnsMany(x => x.Logo, links =>
        {
            links.Property<string>(providerForeignKey);
            DimensionedFormattableItemMapping(links, $"{table}_logos", providerForeignKey);
        });
        builder.OwnsMany(x => x.SeeAlso, links =>
        {
            links.Property<string>(providerForeignKey);
            SeeAlsoMapping(links, $"{table}_see_also", providerForeignKey);
        });

        return builder;
    }

    public static void Metadata<TOwner>(
        OwnedNavigationBuilder<TOwner, Metadata> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable($"{prefix}_metadata");
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Ignore(x => x.HasChanges);
        builder.HasKey(x => x.Label);

        builder.Property(x => x.Label).HasColumnName("label").IsRequired();

        builder.OwnsMany(x => x.Value, value =>
        {
            value.Property<string>("metadata_label");
            MetadataValueMapping(value, $"{prefix}_metadata_values", "metadata_label");
        });
    }

    public static void RequiredStatement<TOwner>(
        OwnedNavigationBuilder<TOwner, RequiredStatement> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable($"{prefix}_required_statement");
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Ignore(x => x.HasChanges);

        // RequiredStatement has no natural id of its own (unlike the BaseItem-derived shapes above), and as an
        // OwnsOne it would otherwise share its owner's key across however many ownership levels deep that owner
        // itself is nested - which EF cannot reliably infer a shadow FK type from for the collections below. A
        // surrogate key decouples it from that chain.
        builder.Property<Guid>("RowId").HasColumnName("row_id").ValueGeneratedOnAdd();
        builder.HasKey("RowId");

        builder.OwnsMany(x => x.Label, label =>
        {
            label.Property<Guid>("required_statement_id");
            LabelMapping(label, $"{prefix}_required_statement_labels", "required_statement_id");
        });
        builder.OwnsMany(x => x.Value, value =>
        {
            value.Property<Guid>("required_statement_id");
            SummaryMapping(value, $"{prefix}_required_statement_values", "required_statement_id");
        });
    }

    /// <summary>
    ///     Configures every property a <see cref="BaseNode{TBaseNode}" /> (<c>Manifest</c>, <c>Structure</c>, ...)
    ///     carries in common: label/summary/metadata/requiredStatement/behavior/homepage/thumbnail/rendering/
    ///     seeAlso/partOf/provider, plus the scalar <c>rights</c> and the JSON-embedded <c>accompanyingCanvas</c>.
    ///     Callers still own the node's own key and its type-specific properties (e.g. <c>Items</c>,
    ///     <c>viewingDirection</c>).
    /// </summary>
    public static OwnedNavigationBuilder<TOwner, TNode> ConfigureBaseNode<TOwner, TNode>(
        OwnedNavigationBuilder<TOwner, TNode> builder,
        string prefix)
        where TOwner : class
        where TNode : BaseNode<TNode>
    {
        builder.Ignore(x => x.HasChanges);
        builder.Ignore(x => x.Service);

        // Legacy (2.x) computed views - Description mirrors Summary, Attribution/License/Within read back from
        // RequiredStatement/Rights/PartOf, ViewingHint falls back onto Behavior, and Related mirrors Homepage.
        // None of these are their own storage, so none may be auto-discovered as navigations/columns.
        builder.Ignore(x => x.Description);
        builder.Ignore(x => x.Attribution);
        builder.Ignore(x => x.License);
        builder.Ignore(x => x.ViewingHint);
        builder.Ignore(x => x.Within);
        builder.Ignore(x => x.Related);

        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100);
        ContextMapping(builder.Property(x => x.Context));

        var fk = $"{prefix}_id";

        builder.OwnsMany(x => x.Label, label => LabelMapping(label, $"{prefix}_labels", fk));
        builder.OwnsMany(x => x.Summary, summary => SummaryMapping(summary, $"{prefix}_summaries", fk));
        builder.OwnsMany(x => x.Metadata, metadata => Metadata(metadata, prefix, fk));
        builder.OwnsOne(x => x.RequiredStatement, statement => RequiredStatement(statement, prefix, fk));
        builder.OwnsMany(x => x.Behavior, behavior => BehaviorMapping(behavior, $"{prefix}_behavior", fk));
        builder.OwnsMany(x => x.Homepage, links => HomepageMapping(links, $"{prefix}_homepage", fk));
        builder.OwnsOne(x => x.Thumbnail, thumbnail => DimensionedFormattableItemMapping(thumbnail, $"{prefix}_thumbnail", fk));
        builder.OwnsOne(x => x.Logo, logo => DimensionedFormattableItemMapping(logo, $"{prefix}_logo", fk));
        builder.OwnsMany(x => x.Rendering, links => RenderingMapping(links, $"{prefix}_rendering", fk));
        builder.OwnsMany(x => x.SeeAlso, links => SeeAlsoMapping(links, $"{prefix}_see_also", fk));
        builder.OwnsMany(x => x.PartOf, links => BaseItemMapping(links, $"{prefix}_part_of", fk));
        builder.OwnsMany(x => x.Provider, agents => ProviderMapping(agents, $"{prefix}_provider", fk));

        builder.Property(x => x.Rights)
            .HasColumnName("rights")
            .HasConversion(x => x!.Value, x => new Rights(x));

        JsonGraph(builder.Property(x => x.AccompanyingCanvas), $"{prefix}_accompanying_canvas");

        return builder;
    }

    //JSON-embedded graphs
    //
    // These node/content shapes are exposed by the SDK only as polymorphic (IBaseItem/interface-typed)
    // or fully-nested-graph collections with no natural relational shape (see the SDK's own docs:
    // "Items is the real backing storage ... 2.x-only shapes are computed views derived from Items").
    // Rather than forcing a relational shape onto them, they round-trip as JSON using the SDK's own
    // Newtonsoft contract resolver - the same reflection-based (de)serialization the SDK relies on
    // internally for its private constructors/setters.

    public static PropertyBuilder<T?> JsonGraph<T>(PropertyBuilder<T?> property, string columnName)
        where T : class =>
        property
            .HasColumnName(columnName)
            .HasColumnType("jsonb")
            .HasConversion(
                value => JsonConvert.SerializeObject(value, IiifJsonSettings.Graph),
                json => JsonConvert.DeserializeObject<T>(json, IiifJsonSettings.Graph));

    /// <summary>
    ///     Manifest.Items always holds <see cref="Canvas" /> entries (the manifest's canvas ordering).
    /// </summary>
    public static PropertyBuilder<IReadOnlyCollection<IBaseItem>> ManifestItemsMapping(
        PropertyBuilder<IReadOnlyCollection<IBaseItem>> property, string columnName)
    {
        property
            .HasColumnName(columnName)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                items => JsonConvert.SerializeObject(items, IiifJsonSettings.Graph),
                json => JsonConvert.DeserializeObject<List<Canvas>>(json, IiifJsonSettings.Graph) ?? new List<Canvas>());
        property.Metadata.SetValueComparer(BaseItemCollectionComparer);

        return property;
    }

    /// <summary>
    ///     Structure.Items can mix CanvasReference/RangeReference/nested Structure entries, so - unlike
    ///     Manifest.Items - the concrete element type isn't fixed; type metadata has to travel with the JSON.
    /// </summary>
    public static PropertyBuilder<IReadOnlyCollection<IBaseItem>> StructureItemsMapping(
        PropertyBuilder<IReadOnlyCollection<IBaseItem>> property, string columnName)
    {
        property
            .HasColumnName(columnName)
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                items => JsonConvert.SerializeObject(items, IiifJsonSettings.Polymorphic),
                json => JsonConvert.DeserializeObject<List<IBaseItem>>(json, IiifJsonSettings.Polymorphic) ?? new List<IBaseItem>());
        property.Metadata.SetValueComparer(BaseItemCollectionComparer);

        return property;
    }
}
