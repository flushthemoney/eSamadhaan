using eSamadhaan.Application.Exceptions;
using System.Net;
using System.Text.Json;

namespace eSamadhaan.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFoundEx => new
            {
                statusCode = (int)HttpStatusCode.NotFound,
                message = notFoundEx.Message,
                details = notFoundEx.Message
            },
            ValidationException validationEx => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = "Validation failed",
                details = validationEx.Message
            },
            UnauthorizedException unauthorizedEx => new
            {
                statusCode = (int)HttpStatusCode.Unauthorized,
                message = unauthorizedEx.Message,
                details = unauthorizedEx.Message
            },
            DuplicateException duplicateEx => new
            {
                statusCode = (int)HttpStatusCode.Conflict,
                message = duplicateEx.Message,
                details = duplicateEx.Message
            },
            InvalidStatusTransitionException statusEx => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = statusEx.Message,
                details = statusEx.Message
            },
            BusinessRuleViolationException businessEx => new
            {
                statusCode = (int)HttpStatusCode.BadRequest,
                message = businessEx.Message,
                details = businessEx.Message
            },
            _ => new
            {
                statusCode = (int)HttpStatusCode.InternalServerError,
                message = "An internal server error occurred",
                details = exception.Message
            }
        };

        context.Response.StatusCode = response.statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
