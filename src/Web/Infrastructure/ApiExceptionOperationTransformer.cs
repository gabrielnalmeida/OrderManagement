using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace OrderManagement.Web.Infrastructure;

/// <summary>
/// Adds baseline error responses to the OpenAPI document for the API surface that will host
/// order endpoints.
/// </summary>
internal sealed class ApiExceptionOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext _, CancellationToken __)
    {
        operation.Responses ??= [];
        operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });

        return Task.CompletedTask;
    }
}
