using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CRM.Infrastructure.Persistence.Contexts;

/// <summary>
/// Factory hỗ trợ chạy Scaffold-DbContext và migrations từ CLI.
/// </summary>
public class CrmDbContextFactory : IDesignTimeDbContextFactory<CrmDbContext>
{
    public CrmDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../CRM.API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var optionsBuilder = new DbContextOptionsBuilder<CrmDbContext>();
        optionsBuilder.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure());

        return new CrmDbContext(optionsBuilder.Options);
    }
}
