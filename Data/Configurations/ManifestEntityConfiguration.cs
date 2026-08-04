using IIIF.POC.PostgreSqlRelationalV3Store.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Data.Configurations;

public sealed class ManifestEntityConfiguration : IEntityTypeConfiguration<ManifestEntity>
{
    public void Configure(EntityTypeBuilder<ManifestEntity> builder)
    {
        builder.ToTable("manifests");
        builder.HasKey(x => x.Id).HasName("pk_manifests");

        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.IiifId).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Context).HasColumnName("context").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.SourceVersion).HasColumnName("source_version").HasMaxLength(32).IsRequired();
        builder.Property(x => x.Rights).HasColumnName("rights").HasMaxLength(2048);
        builder.Property(x => x.NavDate).HasColumnName("nav_date").HasColumnType("timestamp with time zone");
        builder.Property(x => x.ViewingDirection).HasColumnName("viewing_direction").HasMaxLength(32);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.Property(x => x.ContentHash).HasColumnName("content_hash").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => x.IiifId).IsUnique().HasDatabaseName("ux_manifests_iiif_id");
        builder.HasIndex(x => x.UpdatedAtUtc).HasDatabaseName("ix_manifests_updated_at");
        builder.HasIndex(x => x.ContentHash).HasDatabaseName("ix_manifests_hash");
        builder.HasIndex(x => x.AdditionalPropertiesJson).HasMethod("gin").HasDatabaseName("ix_manifests_additional_gin");

        builder.OwnsOne(x => x.Label, label =>
            OwnedMappingHelpers.LanguageMap(label, "m_label_values", "manifest_id"));
        builder.OwnsOne(x => x.Summary, summary =>
            OwnedMappingHelpers.LanguageMap(summary, "m_summary_values", "manifest_id"));
        builder.OwnsMany(x => x.Metadata, metadata =>
            OwnedMappingHelpers.Metadata(metadata, "m", "manifest_id"));
        builder.OwnsOne(x => x.RequiredStatement, statement =>
            OwnedMappingHelpers.RequiredStatement(statement, "m", "manifest_id"));
        builder.OwnsMany(x => x.Behavior, behavior =>
            OwnedMappingHelpers.OrderedStrings(behavior, "m_behavior", "manifest_id"));

        builder.OwnsMany(x => x.Homepage, links => OwnedMappingHelpers.Link(links, "m_homepage", "manifest_id"));
        builder.OwnsMany(x => x.Thumbnail, links => OwnedMappingHelpers.Link(links, "m_thumbnail", "manifest_id"));
        builder.OwnsMany(x => x.Rendering, links => OwnedMappingHelpers.Link(links, "m_rendering", "manifest_id"));
        builder.OwnsMany(x => x.SeeAlso, links => OwnedMappingHelpers.Link(links, "m_see_also", "manifest_id"));
        builder.OwnsMany(x => x.PartOf, links => OwnedMappingHelpers.Link(links, "m_part_of", "manifest_id"));
        builder.OwnsMany(x => x.Provider, agents => OwnedMappingHelpers.Agent(agents, "m_provider", "manifest_id"));
        builder.OwnsMany(x => x.Services, services => OwnedMappingHelpers.Service(services, "m_services", "manifest_id"));

        builder.OwnsMany(x => x.Items, canvases => ConfigureCanvas(canvases, "canvases", "manifest_id", "c"));
        builder.OwnsMany(x => x.Structures, ranges => ConfigureRange(ranges, "ranges", "manifest_id", "r"));
        builder.OwnsMany(x => x.Annotations, pages => ConfigureAnnotationPage(pages, "m_annotation_pages", "manifest_id", "map", true));

        builder.OwnsOne(x => x.PlaceholderCanvas, canvas => ConfigureEmbeddedCanvas(canvas, "m_placeholder_canvas", "manifest_id", "mpc"));
        builder.OwnsOne(x => x.AccompanyingCanvas, canvas => ConfigureEmbeddedCanvas(canvas, "m_accompanying_canvas", "manifest_id", "mac"));
        builder.OwnsOne(x => x.Start, start => ConfigureStart(start, "m_start", "manifest_id", "ms"));
    }

    private static void ConfigureCanvas(
        OwnedNavigationBuilder<ManifestEntity, CanvasEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.IiifId).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Duration).HasColumnName("duration");
        builder.Property(x => x.Rights).HasColumnName("rights").HasMaxLength(2048);
        builder.Property(x => x.NavDate).HasColumnName("nav_date").HasColumnType("timestamp with time zone");
        builder.Property(x => x.ViewingDirection).HasColumnName("viewing_direction").HasMaxLength(32);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(CanvasEntity.Position));
        builder.HasIndex(x => x.IiifId);

        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "canvas_id"));
        builder.OwnsOne(x => x.Summary, summary => OwnedMappingHelpers.LanguageMap(summary, $"{prefix}_summary_values", "canvas_id"));
        builder.OwnsMany(x => x.Metadata, metadata => OwnedMappingHelpers.Metadata(metadata, prefix, "canvas_id"));
        builder.OwnsOne(x => x.RequiredStatement, statement => OwnedMappingHelpers.RequiredStatement(statement, prefix, "canvas_id"));
        builder.OwnsMany(x => x.Behavior, behavior => OwnedMappingHelpers.OrderedStrings(behavior, $"{prefix}_behavior", "canvas_id"));
        builder.OwnsMany(x => x.Homepage, links => OwnedMappingHelpers.Link(links, $"{prefix}_homepage", "canvas_id"));
        builder.OwnsMany(x => x.Thumbnail, links => OwnedMappingHelpers.Link(links, $"{prefix}_thumbnail", "canvas_id"));
        builder.OwnsMany(x => x.Rendering, links => OwnedMappingHelpers.Link(links, $"{prefix}_rendering", "canvas_id"));
        builder.OwnsMany(x => x.SeeAlso, links => OwnedMappingHelpers.Link(links, $"{prefix}_see_also", "canvas_id"));
        builder.OwnsMany(x => x.PartOf, links => OwnedMappingHelpers.Link(links, $"{prefix}_part_of", "canvas_id"));
        builder.OwnsMany(x => x.Provider, agents => OwnedMappingHelpers.Agent(agents, $"{prefix}_provider", "canvas_id"));
        builder.OwnsMany(x => x.Services, services => OwnedMappingHelpers.Service(services, $"{prefix}_services", "canvas_id"));
        builder.OwnsMany(x => x.Items, pages => ConfigureAnnotationPage(pages, $"{prefix}_item_pages", "canvas_id", $"{prefix}ip", true));
        builder.OwnsMany(x => x.Annotations, pages => ConfigureAnnotationPage(pages, $"{prefix}_annotation_pages", "canvas_id", $"{prefix}ap", true));
        builder.OwnsOne(x => x.PlaceholderCanvas, canvas => ConfigureEmbeddedCanvas(canvas, $"{prefix}_placeholder_canvas", "canvas_id", $"{prefix}pc"));
        builder.OwnsOne(x => x.AccompanyingCanvas, canvas => ConfigureEmbeddedCanvas(canvas, $"{prefix}_accompanying_canvas", "canvas_id", $"{prefix}ac"));
    }

    private static void ConfigureEmbeddedCanvas<TOwner>(
        OwnedNavigationBuilder<TOwner, EmbeddedCanvasEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Property(x => x.IiifId).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Duration).HasColumnName("duration");
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "canvas_id"));
        builder.OwnsMany(x => x.Items, pages => ConfigureAnnotationPage(pages, $"{prefix}_item_pages", "canvas_id", $"{prefix}ip", true));
    }

    private static void ConfigureAnnotationPage<TOwner>(
        OwnedNavigationBuilder<TOwner, AnnotationPageEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix,
        bool allowBodyResourceAnnotations)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.IiifId).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(AnnotationPageEntity.Position));
        builder.OwnsMany(x => x.Items, annotations => ConfigureAnnotation(annotations, $"{prefix}_annotations", "annotation_page_id", $"{prefix}a", allowBodyResourceAnnotations));
    }

    private static void ConfigureAnnotation<TOwner>(
        OwnedNavigationBuilder<TOwner, AnnotationEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix,
        bool allowBodyResourceAnnotations)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.IiifId).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.TimeMode).HasColumnName("time_mode").HasMaxLength(100);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(AnnotationEntity.Position));
        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "annotation_id"));
        builder.OwnsMany(x => x.Motivation, motivation => OwnedMappingHelpers.OrderedStrings(motivation, $"{prefix}_motivation", "annotation_id"));
        builder.OwnsMany(x => x.Bodies, bodies => ConfigureContentResource(bodies, $"{prefix}_bodies", "annotation_id", $"{prefix}b", allowBodyResourceAnnotations));
        builder.OwnsMany(x => x.Targets, targets => ConfigureTarget(targets, $"{prefix}_targets", "annotation_id", $"{prefix}t"));
    }

    private static void ConfigureContentResource<TOwner>(
        OwnedNavigationBuilder<TOwner, ContentResourceEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix,
        bool includeAnnotations)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Format).HasColumnName("format").HasMaxLength(255);
        builder.Property(x => x.Profile).HasColumnName("profile").HasMaxLength(2048);
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Duration).HasColumnName("duration");
        builder.Property(x => x.Value).HasColumnName("value");
        builder.Property(x => x.TextDirection).HasColumnName("text_direction").HasMaxLength(16);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(ContentResourceEntity.Position));

        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "resource_id"));
        builder.OwnsMany(x => x.Language, language => OwnedMappingHelpers.OrderedStrings(language, $"{prefix}_languages", "resource_id"));
        builder.OwnsMany(x => x.Behavior, behavior => OwnedMappingHelpers.OrderedStrings(behavior, $"{prefix}_behavior", "resource_id"));
        builder.OwnsMany(x => x.Services, services => OwnedMappingHelpers.Service(services, $"{prefix}_services", "resource_id"));
        if (includeAnnotations)
            builder.OwnsMany(x => x.Annotations, pages => ConfigureAnnotationPage(pages, $"{prefix}_annotation_pages", "resource_id", $"{prefix}ap", false));
        else
            builder.Ignore(x => x.Annotations);
        builder.OwnsMany(x => x.Items, items => ConfigureChoiceItem(items, $"{prefix}_choice_items", "resource_id", $"{prefix}ci"));
        builder.OwnsOne(x => x.SpecificResource, specific => ConfigureSpecificResource(specific, $"{prefix}_specific", "resource_id", $"{prefix}sr"));
    }

    private static void ConfigureChoiceItem<TOwner>(
        OwnedNavigationBuilder<TOwner, ChoiceItemEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Format).HasColumnName("format").HasMaxLength(255);
        builder.Property(x => x.Profile).HasColumnName("profile").HasMaxLength(2048);
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Duration).HasColumnName("duration");
        builder.Property(x => x.Value).HasColumnName("value");
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(ChoiceItemEntity.Position));
        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "choice_item_id"));
        builder.OwnsMany(x => x.Language, language => OwnedMappingHelpers.OrderedStrings(language, $"{prefix}_languages", "choice_item_id"));
        builder.OwnsMany(x => x.Services, services => OwnedMappingHelpers.Service(services, $"{prefix}_services", "choice_item_id"));
    }

    private static void ConfigureSpecificResource<TOwner>(
        OwnedNavigationBuilder<TOwner, SpecificResourceEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceId).HasColumnName("source_id").HasMaxLength(2048);
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(100);
        builder.Property(x => x.StyleClass).HasColumnName("style_class").HasMaxLength(255);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.OwnsOne(x => x.Selector, selector => OwnedMappingHelpers.Selector(selector, $"{prefix}_selector", "specific_resource_id"));
    }

    private static void ConfigureTarget<TOwner>(
        OwnedNavigationBuilder<TOwner, TargetEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100);
        builder.Property(x => x.SourceId).HasColumnName("source_id").HasMaxLength(2048);
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(100);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(TargetEntity.Position));
        builder.OwnsOne(x => x.Selector, selector => OwnedMappingHelpers.Selector(selector, $"{prefix}_selector", "target_id"));
    }

    private static void ConfigureRange(
        OwnedNavigationBuilder<ManifestEntity, RangeEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.IiifId).HasColumnName("iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Rights).HasColumnName("rights").HasMaxLength(2048);
        builder.Property(x => x.NavDate).HasColumnName("nav_date").HasColumnType("timestamp with time zone");
        builder.Property(x => x.ViewingDirection).HasColumnName("viewing_direction").HasMaxLength(32);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(RangeEntity.Position));
        builder.HasIndex(x => x.IiifId);

        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "range_id"));
        builder.OwnsOne(x => x.Summary, summary => OwnedMappingHelpers.LanguageMap(summary, $"{prefix}_summary_values", "range_id"));
        builder.OwnsMany(x => x.Metadata, metadata => OwnedMappingHelpers.Metadata(metadata, prefix, "range_id"));
        builder.OwnsOne(x => x.RequiredStatement, statement => OwnedMappingHelpers.RequiredStatement(statement, prefix, "range_id"));
        builder.OwnsMany(x => x.Behavior, behavior => OwnedMappingHelpers.OrderedStrings(behavior, $"{prefix}_behavior", "range_id"));
        builder.OwnsMany(x => x.Homepage, links => OwnedMappingHelpers.Link(links, $"{prefix}_homepage", "range_id"));
        builder.OwnsMany(x => x.Thumbnail, links => OwnedMappingHelpers.Link(links, $"{prefix}_thumbnail", "range_id"));
        builder.OwnsMany(x => x.Rendering, links => OwnedMappingHelpers.Link(links, $"{prefix}_rendering", "range_id"));
        builder.OwnsMany(x => x.SeeAlso, links => OwnedMappingHelpers.Link(links, $"{prefix}_see_also", "range_id"));
        builder.OwnsMany(x => x.PartOf, links => OwnedMappingHelpers.Link(links, $"{prefix}_part_of", "range_id"));
        builder.OwnsMany(x => x.Provider, agents => OwnedMappingHelpers.Agent(agents, $"{prefix}_provider", "range_id"));
        builder.OwnsMany(x => x.Services, services => OwnedMappingHelpers.Service(services, $"{prefix}_services", "range_id"));
        builder.OwnsMany(x => x.Items, items => ConfigureRangeItem(items, $"{prefix}_items", "range_id", $"{prefix}i"));
        builder.OwnsMany(x => x.Annotations, pages => ConfigureAnnotationPage(pages, $"{prefix}_annotation_pages", "range_id", $"{prefix}ap", true));
        builder.OwnsOne(x => x.Supplementary, link => OwnedMappingHelpers.LinkOne(link, $"{prefix}_supplementary", "range_id"));
    }

    private static void ConfigureRangeItem<TOwner>(
        OwnedNavigationBuilder<TOwner, RangeItemEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceId).HasColumnName("source_id").HasMaxLength(2048);
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(100);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(RangeItemEntity.Position));
        builder.OwnsOne(x => x.Label, label => OwnedMappingHelpers.LanguageMap(label, $"{prefix}_label_values", "range_item_id"));
        builder.OwnsOne(x => x.Selector, selector => OwnedMappingHelpers.Selector(selector, $"{prefix}_selector", "range_item_id"));
    }

    private static void ConfigureStart<TOwner>(
        OwnedNavigationBuilder<TOwner, StartEntity> builder,
        string table,
        string ownerForeignKey,
        string prefix)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.SourceId).HasColumnName("source_id").HasMaxLength(2048);
        builder.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(100);
        OwnedMappingHelpers.Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.OwnsOne(x => x.Selector, selector => OwnedMappingHelpers.Selector(selector, $"{prefix}_selector", "start_id"));
    }
}
