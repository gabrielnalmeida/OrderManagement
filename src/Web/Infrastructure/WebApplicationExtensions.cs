using System.Reflection;
using Asp.Versioning;

namespace OrderManagement.Web.Infrastructure;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Discovers and maps all <see cref="IEndpointGroup"/> implementations in the target assembly.
    /// Each group is registered with a shared route prefix and OpenAPI tag.
    /// </summary>
    public static WebApplication MapEndpoints(this WebApplication app, Assembly assembly)
    {
        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                     && t.IsAssignableTo(typeof(IEndpointGroup)));

        foreach (var type in endpointGroupTypes)
        {
            var groupName = type.Name;
            var version = (int?)type.GetProperty(nameof(IEndpointGroup.Version))?.GetValue(null) ?? 1;

            var routePrefix = type.GetProperty(nameof(IEndpointGroup.RoutePrefix))
                ?.GetValue(null) as string ?? $"/api/v{{version:apiVersion}}/{groupName}";

            var versionSet = app.NewApiVersionSet()
                .HasApiVersion(new ApiVersion(version, 0))
                .ReportApiVersions()
                .Build();

            var group = app.MapGroup(routePrefix)
                .WithTags(groupName)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(new ApiVersion(version, 0));
                
            type.GetMethod(nameof(IEndpointGroup.Map))!.Invoke(null, [group]);
        }

        return app;
    }
}
