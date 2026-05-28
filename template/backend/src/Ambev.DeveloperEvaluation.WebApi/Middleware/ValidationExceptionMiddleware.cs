using Ambev.DeveloperEvaluation.Common.Validation;
using Ambev.DeveloperEvaluation.WebApi.Common;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace Ambev.DeveloperEvaluation.WebApi.Middleware;

public class ValidationExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ValidationExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ValidationExceptionMiddleware(RequestDelegate next, ILogger<ValidationExceptionMiddleware> logger)
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
        catch (ValidationException ex)
        {
            await WriteValidationAsync(context, ex);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain rule violation: {Message}", ex.Message);
            await WriteAsync(context, HttpStatusCode.BadRequest, "DomainError", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteAsync(context, HttpStatusCode.NotFound, "ResourceNotFound", ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteAsync(context, HttpStatusCode.Conflict, "ConflictError", ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteAsync(context, HttpStatusCode.Unauthorized, "AuthenticationError", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception while processing {Path}", context.Request.Path);
            await WriteAsync(context, HttpStatusCode.InternalServerError, "InternalServerError",
                "An unexpected error occurred. Please try again later.");
        }
    }

    private static Task WriteValidationAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var response = new ApiResponse
        {
            Success = false,
            Message = "Validation Failed",
            Errors = exception.Errors.Select(error => (ValidationErrorDetail)error)
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private static Task WriteAsync(HttpContext context, HttpStatusCode status, string type, string detail)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var response = new ApiResponse
        {
            Success = false,
            Message = detail,
            Errors = new[]
            {
                new ValidationErrorDetail { Error = type, Detail = detail }
            }
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}