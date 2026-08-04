using IIIF.POC.PostgreSqlRelationalV3Store.Domain;
using Microsoft.EntityFrameworkCore;

namespace IIIF.POC.PostgreSqlRelationalV3Store.Data;

public sealed class ManifestStoreDbContext(DbContextOptions<ManifestStoreDbContext> options)
    : DbContext(options)
{
    public DbSet<ManifestEntity> Manifests => Set<ManifestEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManifestStoreDbContext).Assembly);
    }
}
