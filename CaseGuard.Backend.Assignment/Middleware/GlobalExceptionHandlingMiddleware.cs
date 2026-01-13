using System.Net;
using System.Text.Json;
using CaseGuard.Backend.Assignment.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CaseGuard.Backend.Assignment.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions
/// and converts them to standardized ProblemDetails responses.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred. Request: {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            BadRequestException badRequest => 
                (HttpStatusCode.BadRequest, "Bad Request", badRequest.Message),
            
            NotFoundException notFound => 
                (HttpStatusCode.NotFound, "Not Found", notFound.Message),
            
            UnauthorizedException unauthorized => 
                (HttpStatusCode.Unauthorized, "Unauthorized", unauthorized.Message),
            
            ForbiddenException forbidden => 
                (HttpStatusCode.Forbidden, "Forbidden", forbidden.Message),
            
            _ => (HttpStatusCode.InternalServerError, "An error occurred while processing your request.", 
                "An unexpected error occurred. Please try again later.")
        };

        var problemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            title,
            status = (int)statusCode,
            detail,
            instance = context.Request.Path,
            timestamp = DateTime.UtcNow
        };

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
