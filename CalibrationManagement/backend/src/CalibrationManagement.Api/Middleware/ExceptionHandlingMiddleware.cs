using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using CalibrationManagement.Application.Common;

namespace CalibrationManagement.Api.Middleware;

/// <summary>Maps every exception to an RFC 7807 problem+json response with a suitable status code.</summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly int[] ConnectivityErrors = { -2, -1, 2, 40, 53, 64, 233, 258, 1231, 4060, 4064, 10053, 10054, 10060, 18452, 18456 };

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Response.StatusCode = 499; // client closed the request
        }
        catch (Exception ex) when (!context.Response.HasStarted)
        {
            var (status, title, detail, errors) = Map(ex);
            if (status >= 500) _logger.LogError(ex, "Unhandled error ({Status}) for {Method} {Path}", status, context.Request.Method, context.Request.Path);
            else _logger.LogInformation("Request rejected ({Status}): {Message}", status, ex.Message);

            var problem = new ProblemDetails { Status = status, Title = title, Detail = detail, Type = $"https://httpstatuses.com/{status}" };
            problem.Extensions["traceId"] = context.TraceIdentifier;
            if (errors is not null) problem.Extensions["errors"] = errors;

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
        }
    }

    public static (int Status, string Title, string Detail, IDictionary<string, string[]>? Errors) Map(Exception ex)
    {
        switch (ex)
        {
            case AppValidationException v:
                return (400, "Validation failed", v.Message, v.Errors);
            case AppException app:
                return (app.StatusCode, Title(app.StatusCode), app.Message, null);
            case BadHttpRequestException bad:
                return (400, "Bad request", bad.Message, null);
            case SqlException sql:
                return MapSql(sql);
            default:
                return (500, "Unexpected error", "An unexpected error occurred. Reference the trace id when contacting support.", null);
        }
    }

    private static (int, string, string, IDictionary<string, string[]>?) MapSql(SqlException sql)
    {
        switch (sql.Number)
        {
            case 50001: return (404, "Not found", sql.Message, null);
            case 50002: return (409, "Conflict", sql.Message, null);
            case 50003: return (400, "Validation failed", sql.Message, new Dictionary<string, string[]> { ["request"] = new[] { sql.Message } });
            case 50004: return (422, "Business rule violation", sql.Message, null);
            case 547: return (409, "Conflict", "The operation conflicts with related data (a database constraint was violated).", null);
            case 2601:
            case 2627: return (409, "Conflict", "A record with the same unique value already exists.", null);
        }
        if (ConnectivityErrors.Contains(sql.Number))
            return (503, "Database unavailable",
                "The API could not reach SQL Server (or login to the database failed). Check that SQL Server is running, the database exists and the connection string in appsettings.Local.json is correct.", null);
        Console.Error.WriteLine(
    $"SQL ERROR: Number={sql.Number}, Message={sql.Message}"
);

        return (
            500,
            "Database error",
            "A database error occurred. Reference the trace id when contacting support.",
            null
        );
    }

    private static string Title(int status) => status switch
    {
        401 => "Unauthorized",
        403 => "Forbidden",
        404 => "Not found",
        409 => "Conflict",
        422 => "Business rule violation",
        _ => "Error"
    };
}
