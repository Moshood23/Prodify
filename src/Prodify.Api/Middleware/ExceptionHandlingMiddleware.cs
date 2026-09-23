using Microsoft.AspNetCore.Mvc;
using Prodify.Application.Common.Exceptions;
using System.Net;
using System.Text.Json;
using ValidationException = Prodify.Application.Common.Exceptions.ValidationException;

namespace Prodify.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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
        context.Response.ContentType = "application/json";

        var (statusCode, title, errors) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred.",
                (object?)validationException.Errors),

            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                notFoundException.Message,
                null),

            ConflictException conflictException => (
                HttpStatusCode.Conflict,
                conflictException.Message,
                null),

            ForbiddenAccessException forbiddenException => (
                HttpStatusCode.Forbidden,
                forbiddenException.Message,
                null),

            BusinessRuleException businessRuleException => (
                HttpStatusCode.BadRequest,
                businessRuleException.Message,
                null),

            // Domain entities guard their rules with ArgumentException / InvalidOperationException.
            // Only treat them as a 400 when they come from Prodify.Domain; anywhere else they are bugs.
            ArgumentException or InvalidOperationException when IsFromDomain(exception) => (
                HttpStatusCode.BadRequest,
                exception.Message,
                null),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                // Never send stack traces to clients outside Development.
                _environment.IsDevelopment() ? new { detail = exception.ToString() } : null)
        };

        if (statusCode == HttpStatusCode.InternalServerError)
            _logger.LogError(exception, "Unhandled exception occurred.");

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Extensions = { ["errors"] = errors }
        };

        var json = JsonSerializer.Serialize(problemDetails);

        await context.Response.WriteAsync(json);
    }

    private static bool IsFromDomain(Exception exception) =>
        exception.TargetSite?.DeclaringType?.Namespace?.StartsWith("Prodify.Domain", StringComparison.Ordinal) == true;
}