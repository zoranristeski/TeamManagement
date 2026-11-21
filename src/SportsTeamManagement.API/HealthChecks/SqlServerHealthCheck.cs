using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SportsTeamManagement.Infrastructure.Data;

namespace SportsTeamManagement.API.HealthChecks;

public sealed class SqlServerHealthCheck(
    ApplicationDbContext dbContext,
    ILogger<SqlServerHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await dbContext.Database.CanConnectAsync(cancellationToken))
            {
                return HealthCheckResult.Unhealthy("Database is unavailable.");
            }

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
            return pendingMigrations.Any()
                ? HealthCheckResult.Unhealthy("Database migrations are pending.")
                : HealthCheckResult.Healthy();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                "Database health check failed ({ExceptionType}).",
                exception.GetType().Name);
            return HealthCheckResult.Unhealthy("Database is unavailable.");
        }
    }
}
