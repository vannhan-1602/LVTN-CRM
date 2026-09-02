using CRM.Infrastructure.Persistence.Contexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CRM.API.Extensions;


public class DatabaseHealthCheck : IHealthCheck
{
    private readonly CrmDbContext _context;

    public DatabaseHealthCheck(CrmDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connection is healthy.")
                : HealthCheckResult.Unhealthy("Cannot connect to database.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database health check threw an exception.", ex);
        }
    }
}