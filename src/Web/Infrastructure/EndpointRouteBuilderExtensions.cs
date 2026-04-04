using System.Diagnostics.CodeAnalysis;

namespace OrderManagement.Web.Infrastructure;

/// <summary>
/// Adds convenience overloads for mapping named Minimal API handlers inside
/// <see cref="IEndpointGroup.Map"/>. Each overload preserves the handler method name as the
/// OpenAPI <c>operationId</c>.
/// <para>
/// <c>pattern</c> is optional for GET and POST, and required for PUT, PATCH, and DELETE.
/// </para>
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <inheritdoc cref="EndpointRouteBuilderExtensions"/>
    public static RouteHandlerBuilder MapGet(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        return builder.MapGet(pattern, handler)
              .WithName(handler.Method.Name);
    }

    /// <inheritdoc cref="EndpointRouteBuilderExtensions"/>
    public static RouteHandlerBuilder MapPost(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern = "")
    {
        Guard.Against.AnonymousMethod(handler);

        return builder.MapPost(pattern, handler)
            .WithName(handler.Method.Name);
    }

    /// <inheritdoc cref="EndpointRouteBuilderExtensions"/>
    public static RouteHandlerBuilder MapPut(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
    {
        Guard.Against.AnonymousMethod(handler);

        return builder.MapPut(pattern, handler)
            .WithName(handler.Method.Name);
    }

    /// <inheritdoc cref="EndpointRouteBuilderExtensions"/>
    public static RouteHandlerBuilder MapPatch(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
    {
        Guard.Against.AnonymousMethod(handler);

        return builder.MapPatch(pattern, handler)
            .WithName(handler.Method.Name);
    }

    /// <inheritdoc cref="EndpointRouteBuilderExtensions"/>
    public static RouteHandlerBuilder MapDelete(this IEndpointRouteBuilder builder, Delegate handler, [StringSyntax("Route")] string pattern)
    {
        Guard.Against.AnonymousMethod(handler);

        return builder.MapDelete(pattern, handler)
            .WithName(handler.Method.Name);
    }
}
