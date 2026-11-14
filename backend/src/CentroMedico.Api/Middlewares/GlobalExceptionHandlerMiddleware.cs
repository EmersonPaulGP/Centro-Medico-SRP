using System.Net;
using System.Text.Json;
using CentroMedico.Application.Common.Models;
using CentroMedico.Domain.Exceptions;

namespace CentroMedico.Api.Middlewares;

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
            _logger.LogError(ex, "Ocurrió una excepción no controlada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                null as List<ApiErrorItem>),

            BusinessRuleException businessEx => (
                HttpStatusCode.BadRequest,
                businessEx.Message,
                null as List<ApiErrorItem>),

            ValidationException validationEx => (
                HttpStatusCode.UnprocessableEntity,
                validationEx.Message,
                validationEx.Errors.SelectMany(kvp =>
                    kvp.Value.Select(err => new ApiErrorItem(kvp.Key, err))).ToList()),

            FluentValidation.ValidationException fluentValidationEx => (
                HttpStatusCode.UnprocessableEntity,
                "Uno o más errores de validación ocurrieron.",
                fluentValidationEx.Errors.Select(e => new ApiErrorItem(e.PropertyName, e.ErrorMessage)).ToList()),

            _ => (
                HttpStatusCode.InternalServerError,
                "Ha ocurrido un error inesperado en el servidor. Por favor, contacte al administrador.",
                null as List<ApiErrorItem>)
        };

        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse<object>.Fail(message, errors);

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var responseJson = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(responseJson);
    }
}
