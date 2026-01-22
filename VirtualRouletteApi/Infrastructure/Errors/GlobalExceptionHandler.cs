using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace VirtualRouletteApi.Infrastructure.Errors;

public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        
        logger.LogError(exception,
            "Unhandled exception. TraceId={TraceId} Path={Path} Method={Method}",
            traceId,
            httpContext.Request.Path.Value,
            httpContext.Request.Method);

        var (status, title) = Map(exception);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.com/{status}",
            Instance = httpContext.Request.Path
        };
        
        problem.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }

    private static (int Status, string Title) Map(Exception ex) =>
        ex switch
        {
            ArgumentException => ((int)HttpStatusCode.BadRequest, ex.Message),
            
            FormatException => ((int)HttpStatusCode.BadRequest, ex.Message),

            InvalidOperationException => ((int)HttpStatusCode.Conflict, ex.Message),

            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),

            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not found"),

            _ => ((int)HttpStatusCode.InternalServerError, "Internal server error")
        };
}