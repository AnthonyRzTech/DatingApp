using System.Net;
using System.Text.Json;

namespace WebMatcha.Middleware;

/// <summary>
/// GlobalExceptionHandler - Middleware pour gérer toutes les exceptions de manière centralisée
/// Améliore la sécurité en évitant les fuites d'informations sensibles
/// </summary>
public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger,
        IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
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
        // Log the exception with full details
        _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            statusCode = context.Response.StatusCode,
            message = GetUserFriendlyMessage(exception),
            details = _env.IsDevelopment() ? exception.ToString() : null,
            timestamp = DateTime.UtcNow
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private string GetUserFriendlyMessage(Exception exception)
    {
        // Don't expose internal error details in production
        if (!_env.IsDevelopment())
        {
            return exception switch
            {
                UnauthorizedAccessException => "Access denied. Please log in.",
                InvalidOperationException => "The requested operation is not valid.",
                ArgumentException => "Invalid request parameters.",
                _ => "An error occurred while processing your request. Please try again later."
            };
        }

        return exception.Message;
    }
}
