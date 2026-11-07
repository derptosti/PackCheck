using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PackCheck.Api.Data;

namespace PackCheck.Tests;

public sealed class PackCheckApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:;Foreign Keys=True");

    public PackCheckApiFactory() => connection.Open();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["CustomVision:Endpoint"] = string.Empty,
                ["CustomVision:PredictionKey"] = string.Empty,
                ["CustomVision:ProjectId"] = string.Empty,
                ["CustomVision:PublishedName"] = string.Empty
            }));
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<PackCheckDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<PackCheckDbContext>>();
            services.AddDbContext<PackCheckDbContext>(options => options.UseSqlite(connection));
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
            connection.Dispose();
    }
}
