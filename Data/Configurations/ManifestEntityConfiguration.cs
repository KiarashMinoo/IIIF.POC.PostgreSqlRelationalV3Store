using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Properties;
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

        builder.OwnsOne(x => x.Manifest, manifest =>
        {
            OwnedMappingHelpers.ConfigureBaseNode(manifest, "manifest");

            // Legacy (2.x) computed views over Items/Structures/Behavior - not their own storage, so they must
            // not be auto-discovered as navigations/collections in their own right.
            manifest.Ignore(x => x.Sequences);
            manifest.Ignore(x => x.AdditionalSequences);
            manifest.Ignore(x => x.Services);

            manifest.Property(x => x.NavDate).HasColumnName("nav_date").HasColumnType("timestamp with time zone");
            manifest.Property(x => x.ViewingDirection)
                .HasColumnName("viewing_direction")
                .HasConversion(x => x!.Value, x => new ViewingDirection(x));

            manifest.OwnsMany(x => x.Structures, structures => ConfigureStructure(structures, "range", "manifest_id"));

            OwnedMappingHelpers.ManifestItemsMapping(manifest.Property(x => x.Items), "items");
            OwnedMappingHelpers.JsonGraph(manifest.Property(x => x.Start), "start");
            OwnedMappingHelpers.JsonGraph(manifest.Property(x => x.PlaceholderCanvas), "placeholder_canvas");

            manifest.HasIndex(x => x.Items).HasMethod("gin").HasDatabaseName("ix_manifests_items_gin");
        });

        builder.Property<string>(ManifestEntity.IiifId).HasColumnName("list_iiif_id").HasMaxLength(2048).IsRequired();
        builder.Property<string>(ManifestEntity.Label).HasColumnName("list_label").HasMaxLength(2048).IsRequired();
        builder.Property<string>(ManifestEntity.SourceVersion).HasColumnName("source_version").HasMaxLength(32).IsRequired();
        builder.Property<string>(ManifestEntity.ContentHash).HasColumnName("content_hash").HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property<int>(ManifestEntity.CanvasCount).HasColumnName("canvas_count").IsRequired();
        builder.Property<int>(ManifestEntity.StructureCount).HasColumnName("structure_count").IsRequired();
        builder.Property<DateTimeOffset>(ManifestEntity.CreatedAtUtc).HasColumnName("created_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property<DateTimeOffset>(ManifestEntity.UpdatedAtUtc).HasColumnName("updated_at_utc").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property<uint>(ManifestEntity.Version).IsRowVersion();

        builder.HasIndex(ManifestEntity.IiifId).IsUnique().HasDatabaseName("ux_manifests_iiif_id");
        builder.HasIndex(ManifestEntity.UpdatedAtUtc).HasDatabaseName("ix_manifests_updated_at");
        builder.HasIndex(ManifestEntity.ContentHash).HasDatabaseName("ix_manifests_hash");
    }

    private static void ConfigureStructure(
        OwnedNavigationBuilder<Manifest, Structure> builder,
        string prefix,
        string ownerForeignKey)
    {
        builder.ToTable($"{prefix}s");
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.Id);

        OwnedMappingHelpers.ConfigureBaseNode(builder, prefix);

        // Legacy (2.x) computed views over Items - not their own storage (see the note on Manifest above).
        builder.Ignore(x => x.Canvases);
        builder.Ignore(x => x.Ranges);
        builder.Ignore(x => x.Members);

        builder.Property(x => x.StartCanvas).HasColumnName("start_canvas").HasMaxLength(2048);
        builder.Property(x => x.ViewingDirection)
            .HasColumnName("viewing_direction")
            .HasConversion(x => x!.Value, x => new ViewingDirection(x));

        OwnedMappingHelpers.StructureItemsMapping(builder.Property(x => x.Items), "items");
    }
}
