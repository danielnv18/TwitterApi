using System.Net;
using System.Text.Json;
using TwitterCloneApi.Application.Common.Exceptions;

namespace TwitterCloneApi.API.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Don't handle if response has already started
        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started, cannot handle exception");
            return; // Can't modify response after it's started
        }

        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                validationException.Errors
            ),
            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                notFoundException.Message,
                null
            ),
            UnauthorizedException unauthorizedException => (
                HttpStatusCode.Unauthorized,
                unauthorizedException.Message,
                null
            ),
            ConflictException conflictException => (
                HttpStatusCode.Conflict,
                conflictException.Message,
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                null
            )
        };

        context.Response.StatusCode = (int)statusCode;

        _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

        var response = new
        {
            StatusCode = (int)statusCode,
            Message = message,
            Errors = errors
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}
