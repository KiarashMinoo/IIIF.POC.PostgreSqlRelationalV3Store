using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using IIIF.Manifests.Serializer;
using IIIF.Manifests.Serializer.Nodes;
using IIIF.Manifests.Serializer.Validation;
using IIIF.POC.PostgreSqlRelationalV3Store.Data;
using IIIF.POC.PostgreSqlRelationalV3Store.Domain;
using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Npgsql;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Services;

public sealed class ManifestStoreService(ManifestStoreDbContext db)
{
    public async Task<IReadOnlyList<ManifestListItem>> ListAsync(string? query, CancellationToken cancellationToken)
    {
        var manifests = db.Manifests.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query))
        {
            var pattern = $"%{query.Trim()}%";
            manifests = manifests.Where(x =>
                EF.Functions.ILike(EF.Property<string>(x, ManifestEntity.IiifId), pattern) ||
                EF.Functions.ILike(EF.Property<string>(x, ManifestEntity.Label), pattern));
        }

        return await manifests
            .OrderByDescending(x => EF.Property<DateTimeOffset>(x, ManifestEntity.UpdatedAtUtc))
            .Select(x => new ManifestListItem(
                x.Id,
                EF.Property<string>(x, ManifestEntity.IiifId),
                EF.Property<string>(x, ManifestEntity.Label),
                EF.Property<string>(x, ManifestEntity.SourceVersion),
                EF.Property<int>(x, ManifestEntity.CanvasCount),
                EF.Property<int>(x, ManifestEntity.StructureCount),
                EF.Property<DateTimeOffset>(x, ManifestEntity.UpdatedAtUtc),
                EF.Property<uint>(x, ManifestEntity.Version)))
            .ToListAsync(cancellationToken);
    }

    public async Task<ManifestDetail?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        var row = await db.Manifests.AsNoTracking().AsSplitQuery()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                IiifId = EF.Property<string>(x, ManifestEntity.IiifId),
                SourceVersion = EF.Property<string>(x, ManifestEntity.SourceVersion),
                ContentHash = EF.Property<string>(x, ManifestEntity.ContentHash),
                CreatedAtUtc = EF.Property<DateTimeOffset>(x, ManifestEntity.CreatedAtUtc),
                UpdatedAtUtc = EF.Property<DateTimeOffset>(x, ManifestEntity.UpdatedAtUtc),
                Version = EF.Property<uint>(x, ManifestEntity.Version),
                x.Manifest
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null) return null;

        return new ManifestDetail(
            row.Id,
            row.IiifId,
            row.SourceVersion,
            row.ContentHash,
            row.CreatedAtUtc,
            row.UpdatedAtUtc,
            row.Version,
            row.Manifest);
    }

    public async Task<ManifestOperationResult> CreateAsync(string json, CancellationToken cancellationToken)
    {
        var prepared = Prepare(json);
        if (!prepared.Result.Succeeded || prepared.Manifest is null)
            return prepared.Result;

        if (await db.Manifests.AnyAsync(x => EF.Property<string>(x, ManifestEntity.IiifId) == prepared.Manifest.Id, cancellationToken))
            return Failure($"A Manifest with IIIF id '{prepared.Manifest.Id}' already exists.", prepared.Result.Findings);

        var now = DateTimeOffset.UtcNow;
        var entity = new ManifestEntity { Id = Guid.NewGuid(), Manifest = prepared.Manifest };
        var entry = db.Manifests.Add(entity);
        entry.Property(ManifestEntity.CreatedAtUtc).CurrentValue = now;
        Stamp(entry, prepared.Manifest, prepared.SourceVersion, now);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Success(entity.Id, prepared.Result.Findings);
        }
        catch (DbUpdateException ex)
        {
            return Failure("PostgreSQL rejected the insert. Check the unique constraints and application logs.",
                prepared.Result.Findings, ex);
        }
    }

    public async Task<ManifestOperationResult> UpdateAsync(
        Guid id,
        uint expectedVersion,
        string json,
        CancellationToken cancellationToken)
    {
        var prepared = Prepare(json);
        if (!prepared.Result.Succeeded || prepared.Manifest is null)
            return prepared.Result;

        var current = await db.Manifests.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { CreatedAtUtc = EF.Property<DateTimeOffset>(x, ManifestEntity.CreatedAtUtc) })
            .SingleOrDefaultAsync(cancellationToken);
        if (current is null)
            return Failure("The Manifest no longer exists.", prepared.Result.Findings);

        if (await db.Manifests.AnyAsync(
                x => x.Id != id && EF.Property<string>(x, ManifestEntity.IiifId) == prepared.Manifest.Id,
                cancellationToken))
        {
            return Failure($"Another stored Manifest already uses IIIF id '{prepared.Manifest.Id}'.",
                prepared.Result.Findings);
        }

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();

            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            await PurgeOwnedRowsAsync(id, cancellationToken);

            var existing = new ManifestEntity { Id = id };
            var entry = db.Manifests.Attach(existing);
            entry.Property(ManifestEntity.Version).OriginalValue = expectedVersion;
            entry.Property(ManifestEntity.CreatedAtUtc).CurrentValue = current.CreatedAtUtc;
            existing.Manifest = prepared.Manifest;
            Stamp(entry, prepared.Manifest, prepared.SourceVersion, DateTimeOffset.UtcNow);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return Success(existing.Id, prepared.Result.Findings);
            }
            catch (DbUpdateConcurrencyException)
            {
                return new ManifestOperationResult
                {
                    ConcurrencyConflict = true,
                    Error = "This Manifest was updated by another request. Reload before saving again.",
                    Findings = prepared.Result.Findings
                };
            }
            catch (DbUpdateException ex)
            {
                return Failure("PostgreSQL rejected the update. Check the application logs.",
                    prepared.Result.Findings, ex);
            }
        });
    }

    public async Task<ManifestOperationResult> DeleteAsync(Guid id, uint expectedVersion, CancellationToken cancellationToken)
    {
        if (!await db.Manifests.AnyAsync(x => x.Id == id, cancellationToken))
            return Failure("The Manifest no longer exists.");

        var existing = new ManifestEntity { Id = id };
        var entry = db.Manifests.Attach(existing);
        entry.Property(ManifestEntity.Version).OriginalValue = expectedVersion;
        db.Manifests.Remove(existing);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Success(id);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new ManifestOperationResult
            {
                ConcurrencyConflict = true,
                Error = "This Manifest changed after the delete page was loaded. Reload before deleting it."
            };
        }
    }

    public async Task<string?> ExportAsync(Guid id, IiifPresentationVersion targetVersion, CancellationToken cancellationToken)
    {
        var manifest = await ManifestNodeAsync(id, cancellationToken);
        return manifest is null ? null : PrettyPrint(IiifSerializer.Serialize(manifest, new IiifSerializerOptions(targetVersion)));
    }

    public async Task<string?> CanonicalJsonAsync(Guid id, CancellationToken cancellationToken)
    {
        var manifest = await ManifestNodeAsync(id, cancellationToken);
        return manifest is null
            ? null
            : PrettyPrint(IiifSerializer.Serialize(manifest, new IiifSerializerOptions(IiifPresentationVersion.V3_0)));
    }

    private Task<Manifest?> ManifestNodeAsync(Guid id, CancellationToken cancellationToken) =>
        db.Manifests.AsNoTracking().AsSplitQuery()
            .Where(x => x.Id == id)
            .Select(x => x.Manifest)
            .SingleOrDefaultAsync(cancellationToken);

    private static readonly string[] ManifestOwnedTables =
    [
        "manifest_behavior", "manifest_homepage", "manifest_labels", "manifest_logo",
        "manifest_metadata", "manifest_part_of", "manifest_provider", "manifest_rendering",
        "manifest_required_statement", "manifest_see_also", "manifest_summaries",
        "manifest_thumbnail", "ranges"
    ];

    // Deletes every row directly owned by this manifest; Postgres cascades the rest
    // (metadata_values, provider_*, required_statement_*, everything under ranges/range_*)
    // since every FK in this schema is ON DELETE CASCADE (verified live via pg_constraint).
    private async Task PurgeOwnedRowsAsync(Guid id, CancellationToken cancellationToken)
    {
        foreach (var table in ManifestOwnedTables)
        {
            // table is always one of the fixed literals in ManifestOwnedTables above, never derived
            // from request input; the only real value (id) is passed as a genuine parameter below.
#pragma warning disable EF1003
            await db.Database.ExecuteSqlRawAsync(
                "DELETE FROM " + table + " WHERE manifest_id = @manifestId",
                [new NpgsqlParameter("manifestId", id)],
                cancellationToken);
#pragma warning restore EF1003
        }
    }

    private static PreparedManifest Prepare(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return PreparedManifest.Failed("Manifest JSON is required.");

        try
        {
            var validation = IiifValidator.ValidateJson(json);
            var findings = validation.Errors
                .Select(x => new ManifestFinding(x.RuleId, x.Severity.ToString(), x.Path, x.Message))
                .ToList();

            if (!validation.IsValid)
                return PreparedManifest.Failed("The Manifest contains validation errors.", findings);

            var sourceVersion = IiifPresentationVersionDetector.Detect(json).ToString();
            var manifest = IiifSerializer.DeserializeManifest(json);

            return PreparedManifest.Succeeded(manifest, sourceVersion, findings);
        }
        catch (Exception ex) when (
            ex is JsonException ||
            ex is Newtonsoft.Json.JsonException ||
            ex is NotSupportedException ||
            ex is ArgumentException)
        {
            return PreparedManifest.Failed($"The Manifest could not be parsed: {ex.Message}");
        }
    }

    private static void Stamp(EntityEntry<ManifestEntity> entry, Manifest manifest, string sourceVersion, DateTimeOffset updatedAtUtc)
    {
        var canonical = IiifSerializer.Serialize(manifest, new IiifSerializerOptions(IiifPresentationVersion.V3_0));
        entry.Property(ManifestEntity.IiifId).CurrentValue = manifest.Id;
        entry.Property(ManifestEntity.Label).CurrentValue = IiifLabelFormatter.FirstOrDefault(manifest.Label);
        entry.Property(ManifestEntity.SourceVersion).CurrentValue = sourceVersion;
        entry.Property(ManifestEntity.ContentHash).CurrentValue = ComputeHash(canonical);
        entry.Property(ManifestEntity.CanvasCount).CurrentValue = manifest.Items.Count;
        entry.Property(ManifestEntity.StructureCount).CurrentValue = manifest.Structures.Count;
        entry.Property(ManifestEntity.UpdatedAtUtc).CurrentValue = updatedAtUtc;
    }

    private static string PrettyPrint(string json)
    {
        using var document = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
    }

    private static string ComputeHash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private static ManifestOperationResult Success(Guid id, IReadOnlyList<ManifestFinding>? findings = null) =>
        new() { Succeeded = true, Id = id, Findings = findings ?? [] };

    private static ManifestOperationResult Failure(
        string error,
        IReadOnlyList<ManifestFinding>? findings = null,
        Exception? exception = null) =>
        new() { Error = exception is null ? error : $"{error} ({exception.GetBaseException().GetType().Name})", Findings = findings ?? [] };

    private sealed record PreparedManifest(ManifestOperationResult Result, Manifest? Manifest, string SourceVersion = "")
    {
        public static PreparedManifest Succeeded(Manifest manifest, string sourceVersion, IReadOnlyList<ManifestFinding> findings) =>
            new(new ManifestOperationResult { Succeeded = true, Findings = findings }, manifest, sourceVersion);

        public static PreparedManifest Failed(string error, IReadOnlyList<ManifestFinding>? findings = null) =>
            new(new ManifestOperationResult { Error = error, Findings = findings ?? [] }, null);
    }
}