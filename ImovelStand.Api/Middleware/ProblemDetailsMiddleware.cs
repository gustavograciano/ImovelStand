using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace ImovelStand.Api.Middleware;

/// <summary>
/// Captura exceções não-tratadas e as serializa como ProblemDetails (RFC 7807),
/// com traceId correlacionável com logs/APM.
/// </summary>
public class ProblemDetailsMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ProblemDetailsMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger, IHostEnvironment env)
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
        catch (ValidationException ex)
        {
            await WriteValidationProblemAsync(context, ex);
        }
        catch (Exception ex)
        {
            await WriteProblemAsync(context, ex);
        }
    }

    private async Task WriteValidationProblemAsync(HttpContext context, ValidationException ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

        var problem = new ValidationProblemDetails(errors)
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Erro de validação.",
            Status = (int)HttpStatusCode.BadRequest,
            Detail = "Um ou mais campos falharam na validação.",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = traceId;

        _logger.LogWarning(ex, "Validação falhou em {Path}", context.Request.Path);

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, JsonOptions);
    }

    private async Task WriteProblemAsync(HttpContext context, Exception ex)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        _logger.LogError(ex, "Exceção não-tratada em {Path} (traceId={TraceId})", context.Request.Path, traceId);

        var problem = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Erro inesperado no servidor.",
            Status = (int)HttpStatusCode.InternalServerError,
            Detail = _env.IsDevelopment() ? ex.ToString() : "Contate o suporte e informe o traceId.",
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = traceId;

        context.Response.StatusCode = problem.Status.Value;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, JsonOptions);
    }
}
