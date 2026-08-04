using System.Security.Cryptography;
using System.Text;
using IIIF.Manifests.Serializer;
using IIIF.Manifests.Serializer.Validation;
using IIIF.POC.PostgreSqlRelationalV3Store.Data;
using IIIF.POC.PostgreSqlRelationalV3Store.Domain;
using IIIF.POC.PostgreSqlRelationalV3Store.Models;
using Microsoft.EntityFrameworkCore;

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
                EF.Functions.ILike(x.IiifId, pattern) ||
                x.Label.Values.Any(v => EF.Functions.ILike(v.Value, pattern)));
        }

        return await manifests
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Select(x => new ManifestListItem(
                x.Id,
                x.IiifId,
                x.Label.Values.OrderBy(v => v.Position).Select(v => v.Value).FirstOrDefault() ?? "Untitled Manifest",
                x.SourceVersion,
                x.Items.Count,
                x.Structures.Count,
                x.UpdatedAtUtc,
                x.Version))
            .ToListAsync(cancellationToken);
    }

    public Task<ManifestEntity?> FindAsync(Guid id, CancellationToken cancellationToken) =>
        db.Manifests.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<ManifestOperationResult> CreateAsync(string json, CancellationToken cancellationToken)
    {
        var prepared = Prepare(json);
        if (!prepared.Result.Succeeded || prepared.Entity is null)
            return prepared.Result;

        if (await db.Manifests.AnyAsync(x => x.IiifId == prepared.Entity.IiifId, cancellationToken))
            return Failure($"A Manifest with IIIF id '{prepared.Entity.IiifId}' already exists.", prepared.Result.Findings);

        var now = DateTimeOffset.UtcNow;
        prepared.Entity.Id = Guid.NewGuid();
        prepared.Entity.CreatedAtUtc = now;
        prepared.Entity.UpdatedAtUtc = now;
        prepared.Entity.ContentHash = ComputeHash(ManifestRelationalMapper.ToCanonicalJson(prepared.Entity));
        db.Manifests.Add(prepared.Entity);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return Success(prepared.Entity.Id, prepared.Result.Findings);
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
        if (!prepared.Result.Succeeded || prepared.Entity is null)
            return prepared.Result;

        var existing = await db.Manifests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null)
            return Failure("The Manifest no longer exists.", prepared.Result.Findings);

        if (await db.Manifests.AnyAsync(
                x => x.Id != id && x.IiifId == prepared.Entity.IiifId,
                cancellationToken))
        {
            return Failure($"Another stored Manifest already uses IIIF id '{prepared.Entity.IiifId}'.",
                prepared.Result.Findings);
        }

        db.Entry(existing).Property(x => x.Version).OriginalValue = expectedVersion;
        ReplaceAggregate(existing, prepared.Entity);
        existing.UpdatedAtUtc = DateTimeOffset.UtcNow;
        existing.ContentHash = ComputeHash(ManifestRelationalMapper.ToCanonicalJson(existing));

        try
        {
            await db.SaveChangesAsync(cancellationToken);
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
    }

    public async Task<ManifestOperationResult> DeleteAsync(Guid id, uint expectedVersion, CancellationToken cancellationToken)
    {
        var existing = await db.Manifests.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (existing is null) return Failure("The Manifest no longer exists.");

        db.Entry(existing).Property(x => x.Version).OriginalValue = expectedVersion;
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
        var entity = await db.Manifests.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;

        var canonicalV3 = ManifestRelationalMapper.ToCanonicalJson(entity, indented: true);
        var manifest = IiifSerializer.DeserializeManifest(canonicalV3);
        return IiifSerializer.Serialize(manifest, new IiifSerializerOptions(targetVersion));
    }

    public async Task<string?> CanonicalJsonAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await FindAsync(id, cancellationToken);
        return entity is null ? null : ManifestRelationalMapper.ToCanonicalJson(entity, indented: true);
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
            var canonical = IiifSerializer.Serialize(
                manifest,
                new IiifSerializerOptions(IiifPresentationVersion.V3_0));

            var entity = ManifestRelationalMapper.FromCanonicalJson(canonical, sourceVersion);

            // Verify that the relational projection can rebuild a Manifest accepted by the SDK.
            var rebuilt = ManifestRelationalMapper.ToCanonicalJson(entity);
            _ = IiifSerializer.DeserializeManifest(rebuilt);

            return PreparedManifest.Succeeded(entity, findings);
        }
        catch (Exception ex) when (
            ex is System.Text.Json.JsonException ||
            ex is Newtonsoft.Json.JsonException ||
            ex is NotSupportedException ||
            ex is ArgumentException)
        {
            return PreparedManifest.Failed($"The Manifest could not be parsed or projected: {ex.Message}");
        }
    }

    private static void ReplaceAggregate(ManifestEntity target, ManifestEntity source)
    {
        target.IiifId = source.IiifId;
        target.Type = source.Type;
        target.Context = source.Context;
        target.SourceVersion = source.SourceVersion;
        target.Rights = source.Rights;
        target.NavDate = source.NavDate;
        target.ViewingDirection = source.ViewingDirection;
        target.Label = source.Label;
        target.Summary = source.Summary;
        target.Metadata = source.Metadata;
        target.RequiredStatement = source.RequiredStatement;
        target.Behavior = source.Behavior;
        target.Homepage = source.Homepage;
        target.Thumbnail = source.Thumbnail;
        target.Rendering = source.Rendering;
        target.SeeAlso = source.SeeAlso;
        target.PartOf = source.PartOf;
        target.Provider = source.Provider;
        target.Services = source.Services;
        target.Items = source.Items;
        target.Structures = source.Structures;
        target.Annotations = source.Annotations;
        target.PlaceholderCanvas = source.PlaceholderCanvas;
        target.AccompanyingCanvas = source.AccompanyingCanvas;
        target.Start = source.Start;
        target.AdditionalPropertiesJson = source.AdditionalPropertiesJson;
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

    private sealed record PreparedManifest(ManifestOperationResult Result, ManifestEntity? Entity)
    {
        public static PreparedManifest Succeeded(ManifestEntity entity, IReadOnlyList<ManifestFinding> findings) =>
            new(new ManifestOperationResult { Succeeded = true, Findings = findings }, entity);

        public static PreparedManifest Failed(string error, IReadOnlyList<ManifestFinding>? findings = null) =>
            new(new ManifestOperationResult { Error = error, Findings = findings ?? [] }, null);
    }
}
