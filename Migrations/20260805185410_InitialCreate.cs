using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IIIF.POC.PostgreSqlRelationalV3Store.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "manifests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nav_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    start = table.Column<string>(type: "jsonb", nullable: true),
                    placeholder_canvas = table.Column<string>(type: "jsonb", nullable: true),
                    viewing_direction = table.Column<string>(type: "text", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    iiif_id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rights = table.Column<string>(type: "text", nullable: true),
                    manifest_accompanying_canvas = table.Column<string>(type: "jsonb", nullable: true),
                    items = table.Column<string>(type: "jsonb", nullable: false),
                    canvas_count = table.Column<int>(type: "integer", nullable: false),
                    content_hash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    list_iiif_id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    list_label = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    source_version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    structure_count = table.Column<int>(type: "integer", nullable: false),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_manifests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "manifest_behavior",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_behavior", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_behavior_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_homepage",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "text", nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_homepage", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_homepage_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_labels",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_labels", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_labels_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_logo",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    height = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_logo", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_logo_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_metadata",
                columns: table => new
                {
                    label = table.Column<string>(type: "text", nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_metadata", x => x.label);
                    table.ForeignKey(
                        name: "FK_manifest_metadata_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_part_of",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_part_of", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_part_of_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_provider",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_provider", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_provider_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_rendering",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_rendering", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_rendering_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_required_statement",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_required_statement", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_required_statement_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_see_also",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    profile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    label = table.Column<string>(type: "text", nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_see_also", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_see_also_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_summaries",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_summaries", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_summaries_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_thumbnail",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    height = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_thumbnail", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_thumbnail_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ranges",
                columns: table => new
                {
                    iiif_id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    start_canvas = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    viewing_direction = table.Column<string>(type: "text", nullable: true),
                    manifest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    rights = table.Column<string>(type: "text", nullable: true),
                    range_accompanying_canvas = table.Column<string>(type: "jsonb", nullable: true),
                    items = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ranges", x => x.iiif_id);
                    table.ForeignKey(
                        name: "FK_ranges_manifests_manifest_id",
                        column: x => x.manifest_id,
                        principalTable: "manifests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_metadata_values",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    metadata_label = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_metadata_values", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_metadata_values_manifest_metadata_metadata_label",
                        column: x => x.metadata_label,
                        principalTable: "manifest_metadata",
                        principalColumn: "label",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_provider_homepages",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "text", nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_provider_homepages", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_provider_homepages_manifest_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "manifest_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_provider_labels",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_provider_labels", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_provider_labels_manifest_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "manifest_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_provider_logos",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    height = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_provider_logos", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_provider_logos_manifest_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "manifest_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_provider_see_also",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    profile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    label = table.Column<string>(type: "text", nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: false),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_provider_see_also", x => x.id);
                    table.ForeignKey(
                        name: "FK_manifest_provider_see_also_manifest_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "manifest_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_required_statement_labels",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    required_statement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_required_statement_labels", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_required_statement_labels_manifest_required_statem~",
                        column: x => x.required_statement_id,
                        principalTable: "manifest_required_statement",
                        principalColumn: "row_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "manifest_required_statement_values",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    required_statement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_manifest_required_statement_values", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_manifest_required_statement_values_manifest_required_statem~",
                        column: x => x.required_statement_id,
                        principalTable: "manifest_required_statement",
                        principalColumn: "row_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_behavior",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_behavior", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_behavior_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_homepage",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "text", nullable: true),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_homepage", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_homepage_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_labels",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_labels", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_labels_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_logo",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    height = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_logo", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_logo_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_metadata",
                columns: table => new
                {
                    label = table.Column<string>(type: "text", nullable: false),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_metadata", x => x.label);
                    table.ForeignKey(
                        name: "FK_range_metadata_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_part_of",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_part_of", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_part_of_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_provider",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_provider", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_provider_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_rendering",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "text", nullable: false),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_rendering", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_rendering_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_required_statement",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_required_statement", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_required_statement_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_see_also",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    profile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    label = table.Column<string>(type: "text", nullable: true),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_see_also", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_see_also_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_summaries",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_summaries", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_summaries_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_thumbnail",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    height = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    range_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_thumbnail", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_thumbnail_ranges_range_id",
                        column: x => x.range_id,
                        principalTable: "ranges",
                        principalColumn: "iiif_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_metadata_values",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    metadata_label = table.Column<string>(type: "text", nullable: true),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_metadata_values", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_metadata_values_range_metadata_metadata_label",
                        column: x => x.metadata_label,
                        principalTable: "range_metadata",
                        principalColumn: "label",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_provider_homepages",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    label = table.Column<string>(type: "text", nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_provider_homepages", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_provider_homepages_range_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "range_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_provider_labels",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_provider_labels", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_provider_labels_range_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "range_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_provider_logos",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    height = table.Column<int>(type: "integer", nullable: true),
                    width = table.Column<int>(type: "integer", nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_provider_logos", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_provider_logos_range_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "range_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_provider_see_also",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    profile = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    label = table.Column<string>(type: "text", nullable: true),
                    provider_id = table.Column<string>(type: "character varying(2048)", nullable: true),
                    context = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    format = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_provider_see_also", x => x.id);
                    table.ForeignKey(
                        name: "FK_range_provider_see_also_range_provider_provider_id",
                        column: x => x.provider_id,
                        principalTable: "range_provider",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_required_statement_labels",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    required_statement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_required_statement_labels", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_required_statement_labels_range_required_statement_re~",
                        column: x => x.required_statement_id,
                        principalTable: "range_required_statement",
                        principalColumn: "row_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "range_required_statement_values",
                columns: table => new
                {
                    row_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    required_statement_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_range_required_statement_values", x => x.row_id);
                    table.ForeignKey(
                        name: "FK_range_required_statement_values_range_required_statement_re~",
                        column: x => x.required_statement_id,
                        principalTable: "range_required_statement",
                        principalColumn: "row_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_manifest_behavior_manifest_id",
                table: "manifest_behavior",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_homepage_manifest_id",
                table: "manifest_homepage",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_labels_manifest_id",
                table: "manifest_labels",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_logo_manifest_id",
                table: "manifest_logo",
                column: "manifest_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manifest_metadata_manifest_id",
                table: "manifest_metadata",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_metadata_values_metadata_label",
                table: "manifest_metadata_values",
                column: "metadata_label");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_part_of_manifest_id",
                table: "manifest_part_of",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_provider_manifest_id",
                table: "manifest_provider",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_provider_homepages_provider_id",
                table: "manifest_provider_homepages",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_provider_labels_provider_id",
                table: "manifest_provider_labels",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_provider_logos_provider_id",
                table: "manifest_provider_logos",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_provider_see_also_provider_id",
                table: "manifest_provider_see_also",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_rendering_manifest_id",
                table: "manifest_rendering",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_required_statement_manifest_id",
                table: "manifest_required_statement",
                column: "manifest_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_manifest_required_statement_labels_required_statement_id",
                table: "manifest_required_statement_labels",
                column: "required_statement_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_required_statement_values_required_statement_id",
                table: "manifest_required_statement_values",
                column: "required_statement_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_see_also_manifest_id",
                table: "manifest_see_also",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_summaries_manifest_id",
                table: "manifest_summaries",
                column: "manifest_id");

            migrationBuilder.CreateIndex(
                name: "IX_manifest_thumbnail_manifest_id",
                table: "manifest_thumbnail",
                column: "manifest_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_manifests_hash",
                table: "manifests",
                column: "content_hash");

            migrationBuilder.CreateIndex(
                name: "ix_manifests_items_gin",
                table: "manifests",
                column: "items")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "ix_manifests_updated_at",
                table: "manifests",
                column: "updated_at_utc");

            migrationBuilder.CreateIndex(
                name: "ux_manifests_iiif_id",
                table: "manifests",
                column: "list_iiif_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_range_behavior_range_id",
                table: "range_behavior",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_homepage_range_id",
                table: "range_homepage",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_labels_range_id",
                table: "range_labels",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_logo_range_id",
                table: "range_logo",
                column: "range_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_range_metadata_range_id",
                table: "range_metadata",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_metadata_values_metadata_label",
                table: "range_metadata_values",
                column: "metadata_label");

            migrationBuilder.CreateIndex(
                name: "IX_range_part_of_range_id",
                table: "range_part_of",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_provider_range_id",
                table: "range_provider",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_provider_homepages_provider_id",
                table: "range_provider_homepages",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_provider_labels_provider_id",
                table: "range_provider_labels",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_provider_logos_provider_id",
                table: "range_provider_logos",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_provider_see_also_provider_id",
                table: "range_provider_see_also",
                column: "provider_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_rendering_range_id",
                table: "range_rendering",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_required_statement_range_id",
                table: "range_required_statement",
                column: "range_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_range_required_statement_labels_required_statement_id",
                table: "range_required_statement_labels",
                column: "required_statement_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_required_statement_values_required_statement_id",
                table: "range_required_statement_values",
                column: "required_statement_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_see_also_range_id",
                table: "range_see_also",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_summaries_range_id",
                table: "range_summaries",
                column: "range_id");

            migrationBuilder.CreateIndex(
                name: "IX_range_thumbnail_range_id",
                table: "range_thumbnail",
                column: "range_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ranges_manifest_id",
                table: "ranges",
                column: "manifest_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "manifest_behavior");

            migrationBuilder.DropTable(
                name: "manifest_homepage");

            migrationBuilder.DropTable(
                name: "manifest_labels");

            migrationBuilder.DropTable(
                name: "manifest_logo");

            migrationBuilder.DropTable(
                name: "manifest_metadata_values");

            migrationBuilder.DropTable(
                name: "manifest_part_of");

            migrationBuilder.DropTable(
                name: "manifest_provider_homepages");

            migrationBuilder.DropTable(
                name: "manifest_provider_labels");

            migrationBuilder.DropTable(
                name: "manifest_provider_logos");

            migrationBuilder.DropTable(
                name: "manifest_provider_see_also");

            migrationBuilder.DropTable(
                name: "manifest_rendering");

            migrationBuilder.DropTable(
                name: "manifest_required_statement_labels");

            migrationBuilder.DropTable(
                name: "manifest_required_statement_values");

            migrationBuilder.DropTable(
                name: "manifest_see_also");

            migrationBuilder.DropTable(
                name: "manifest_summaries");

            migrationBuilder.DropTable(
                name: "manifest_thumbnail");

            migrationBuilder.DropTable(
                name: "range_behavior");

            migrationBuilder.DropTable(
                name: "range_homepage");

            migrationBuilder.DropTable(
                name: "range_labels");

            migrationBuilder.DropTable(
                name: "range_logo");

            migrationBuilder.DropTable(
                name: "range_metadata_values");

            migrationBuilder.DropTable(
                name: "range_part_of");

            migrationBuilder.DropTable(
                name: "range_provider_homepages");

            migrationBuilder.DropTable(
                name: "range_provider_labels");

            migrationBuilder.DropTable(
                name: "range_provider_logos");

            migrationBuilder.DropTable(
                name: "range_provider_see_also");

            migrationBuilder.DropTable(
                name: "range_rendering");

            migrationBuilder.DropTable(
                name: "range_required_statement_labels");

            migrationBuilder.DropTable(
                name: "range_required_statement_values");

            migrationBuilder.DropTable(
                name: "range_see_also");

            migrationBuilder.DropTable(
                name: "range_summaries");

            migrationBuilder.DropTable(
                name: "range_thumbnail");

            migrationBuilder.DropTable(
                name: "manifest_metadata");

            migrationBuilder.DropTable(
                name: "manifest_provider");

            migrationBuilder.DropTable(
                name: "manifest_required_statement");

            migrationBuilder.DropTable(
                name: "range_metadata");

            migrationBuilder.DropTable(
                name: "range_provider");

            migrationBuilder.DropTable(
                name: "range_required_statement");

            migrationBuilder.DropTable(
                name: "ranges");

            migrationBuilder.DropTable(
                name: "manifests");
        }
    }
}
