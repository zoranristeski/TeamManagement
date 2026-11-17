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
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Database is unavailable.");
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
