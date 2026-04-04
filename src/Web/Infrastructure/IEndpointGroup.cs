namespace OrderManagement.Web.Infrastructure;

/// <summary>
/// Defines a group of related versioned API endpoints.
/// Implementations are discovered automatically and registered under a shared route prefix and
/// OpenAPI tag.
/// </summary>
public interface IEndpointGroup
{
    /// <summary>
    /// Route prefix used when mapping the endpoint group.
    /// When omitted, the default prefix is <c>/api/{ClassName}</c>.
    /// </summary>
    static virtual string? RoutePrefix => null;

    static abstract void Map(RouteGroupBuilder groupBuilder);
}
