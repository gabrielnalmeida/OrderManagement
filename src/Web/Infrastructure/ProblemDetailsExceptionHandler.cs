using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common.Exceptions;

namespace OrderManagement.Web.Infrastructure;

/// <summary>
/// Converts known application failures into <see cref="ProblemDetails"/> responses.
/// Unknown exceptions are left for the remaining ASP.NET Core error pipeline.
/// </summary>
public class ProblemDetailsExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException ve => (StatusCodes.Status400BadRequest, new ValidationProblemDetails(ve.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = ve.Errors.SelectMany(x => x.Value).FirstOrDefault()
                    ?? exception.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            }),
            NotFoundException nfe => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Resource Not Found",
                Detail = nfe.Message,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            }),
            _ => (-1, null)
        };

        if (problemDetails is null) return false;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
