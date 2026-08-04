using IIIF.POC.PostgreSqlRelationalV3Store.Data;
using IIIF.POC.PostgreSqlRelationalV3Store.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("PostgreSql")
    ?? throw new InvalidOperationException("Connection string 'PostgreSql' is not configured.");

builder.Services.AddRazorPages();
builder.Services.AddDbContext<ManifestStoreDbContext>(options =>
    options.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(3), null)));
builder.Services.AddScoped<ManifestStoreService>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();

// POC convenience only. Replace with migrations before production use.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ManifestStoreDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.Run();
