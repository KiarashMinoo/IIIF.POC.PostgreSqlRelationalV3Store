using IIIF.POC.PostgreSqlRelationalV3Store.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Data.Configurations;

internal static class OwnedMappingHelpers
{
    public static void LanguageMap<TOwner>(
        OwnedNavigationBuilder<TOwner, LanguageMapEntity> builder,
        string table,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.OwnsMany(x => x.Values, values =>
        {
            values.ToTable(table);
            values.WithOwner().HasForeignKey(ownerForeignKey);
            values.HasKey(x => x.RowId);
            values.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
            values.Property(x => x.Position).HasColumnName("position").IsRequired();
            values.Property(x => x.Language).HasColumnName("language").HasMaxLength(35).IsRequired();
            values.Property(x => x.Value).HasColumnName("value").IsRequired();
            values.HasIndex(ownerForeignKey, nameof(LanguageValueEntity.Position));
        });
    }

    public static void OrderedStrings<TOwner>(
        OwnedNavigationBuilder<TOwner, OrderedStringEntity> builder,
        string table,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Value).HasColumnName("value").IsRequired();
        builder.HasIndex(ownerForeignKey, nameof(OrderedStringEntity.Position));
    }

    public static void Metadata<TOwner>(
        OwnedNavigationBuilder<TOwner, MetadataEntity> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable($"{prefix}_metadata");
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.HasIndex(ownerForeignKey, nameof(MetadataEntity.Position));

        builder.OwnsOne(x => x.Label, label =>
            LanguageMap(label, $"{prefix}_metadata_label_values", "metadata_id"));
        builder.OwnsOne(x => x.Value, value =>
            LanguageMap(value, $"{prefix}_metadata_value_values", "metadata_id"));
    }

    public static void RequiredStatement<TOwner>(
        OwnedNavigationBuilder<TOwner, RequiredStatementEntity> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable($"{prefix}_required_statement");
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.OwnsOne(x => x.Label, label =>
            LanguageMap(label, $"{prefix}_required_label_values", "required_statement_id"));
        builder.OwnsOne(x => x.Value, value =>
            LanguageMap(value, $"{prefix}_required_value_values", "required_statement_id"));
    }

    public static void Service<TOwner>(
        OwnedNavigationBuilder<TOwner, ServiceEntity> builder,
        string table,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Context).HasColumnName("context").HasMaxLength(2048);
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100);
        builder.Property(x => x.Profile).HasColumnName("profile").HasMaxLength(2048);
        Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(ServiceEntity.Position));
    }

    public static void Link<TOwner>(
        OwnedNavigationBuilder<TOwner, LinkEntity> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable(prefix);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100);
        builder.Property(x => x.Format).HasColumnName("format").HasMaxLength(255);
        builder.Property(x => x.Profile).HasColumnName("profile").HasMaxLength(2048);
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Duration).HasColumnName("duration");
        Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(LinkEntity.Position));

        builder.OwnsOne(x => x.Label, label =>
            LanguageMap(label, $"{prefix}_label_values", "link_id"));
        builder.OwnsMany(x => x.Language, language =>
            OrderedStrings(language, $"{prefix}_languages", "link_id"));
        builder.OwnsMany(x => x.Services, services =>
            Service(services, $"{prefix}_services", "link_id"));
    }


    public static void LinkOne<TOwner>(
        OwnedNavigationBuilder<TOwner, LinkEntity> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable(prefix);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100);
        builder.Property(x => x.Format).HasColumnName("format").HasMaxLength(255);
        builder.Property(x => x.Profile).HasColumnName("profile").HasMaxLength(2048);
        builder.Property(x => x.Height).HasColumnName("height");
        builder.Property(x => x.Width).HasColumnName("width");
        builder.Property(x => x.Duration).HasColumnName("duration");
        Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");

        builder.OwnsOne(x => x.Label, label =>
            LanguageMap(label, $"{prefix}_label_values", "link_id"));
        builder.OwnsMany(x => x.Language, language =>
            OrderedStrings(language, $"{prefix}_languages", "link_id"));
        builder.OwnsMany(x => x.Services, services =>
            Service(services, $"{prefix}_services", "link_id"));
    }

    public static void Agent<TOwner>(
        OwnedNavigationBuilder<TOwner, AgentEntity> builder,
        string prefix,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable(prefix);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.HasKey(x => x.RowId);
        builder.Property(x => x.RowId).HasColumnName("row_id").ValueGeneratedNever();
        builder.Property(x => x.Position).HasColumnName("position").IsRequired();
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
        builder.HasIndex(ownerForeignKey, nameof(AgentEntity.Position));

        builder.OwnsOne(x => x.Label, label =>
            LanguageMap(label, $"{prefix}_label_values", "agent_id"));
        builder.OwnsMany(x => x.Homepage, links => Link(links, $"{prefix}_homepage", "agent_id"));
        builder.OwnsMany(x => x.Logo, links => Link(links, $"{prefix}_logo", "agent_id"));
        builder.OwnsMany(x => x.SeeAlso, links => Link(links, $"{prefix}_see_also", "agent_id"));
        builder.OwnsMany(x => x.Services, services => Service(services, $"{prefix}_services", "agent_id"));
    }

    public static void Selector<TOwner>(
        OwnedNavigationBuilder<TOwner, SelectorEntity> builder,
        string table,
        string ownerForeignKey)
        where TOwner : class
    {
        builder.ToTable(table);
        builder.WithOwner().HasForeignKey(ownerForeignKey);
        builder.Property(x => x.Id).HasColumnName("iiif_id").HasMaxLength(2048);
        builder.Property(x => x.Type).HasColumnName("type").HasMaxLength(100).IsRequired();
        builder.Property(x => x.Value).HasColumnName("value");
        builder.Property(x => x.ConformsTo).HasColumnName("conforms_to").HasMaxLength(2048);
        builder.Property(x => x.X).HasColumnName("x");
        builder.Property(x => x.Y).HasColumnName("y");
        builder.Property(x => x.T).HasColumnName("t");
        Jsonb(builder.Property(x => x.AdditionalPropertiesJson), "additional_properties");
    }

    public static PropertyBuilder<string> Jsonb(PropertyBuilder<string> property, string columnName) =>
        property
            .HasColumnName(columnName)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'{}'::jsonb")
            .IsRequired();
}
