using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OrderManagement.Infrastructure.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
        });

        builder.Services.AddCors();

        builder.Services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(tags: ["ready"])
            .AddCheck("self", static () => HealthCheckResult.Healthy(), ["live"]);
    }
}
