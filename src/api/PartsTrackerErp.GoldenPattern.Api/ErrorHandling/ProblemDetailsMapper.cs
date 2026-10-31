using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using PartsTrackerErp.GoldenPattern.Core.Contracts.Responses;
using PartsTrackerErp.GoldenPattern.Core.Domain.Exceptions;

namespace PartsTrackerErp.GoldenPattern.Api.ErrorHandling;

/// <summary>
/// Builds every failure the API reports as the same RFC 9457 problem: the category in <c>type</c> and
/// <c>status</c>, and an <c>errors</c> array holding the occurrences. Only authored text reaches the caller.
/// </summary>
public static class ProblemDetailsMapper
{
    private const string TypeBase = "https://api.example.com/problems/";
    private const string ErrorsKey = "errors";
    private const string ValidationTitle = "Validation Failed";
    private const string ValidationSummary = "One or more validation errors occurred.";
    private const string GenericSummary = "One or more errors occurred.";
    private const string InvalidRequestCode = "InvalidRequest";
    private const string InvalidRequestDetail = "The request is invalid.";

    private static readonly char[] s_pathSeparators = ['.', '[', ']'];

    // Derived from ErrorTypeExtensions so the ErrorType to status mapping exists once.
    private static readonly Dictionary<int, string> s_categoryTypes = BuildCategoryTypes();

    /// <summary>A problem for a failure with a category: a service failure or a <see cref="DomainException"/>.</summary>
    public static ProblemDetails ToProblemDetails(DomainErrorEnvelope envelope)
    {
        bool validation = envelope.ErrorType == ErrorType.Validation;

        return Create(
            envelope.ErrorType.ToHttpStatusCode(),
            validation ? ValidationTitle : null,
            validation ? ValidationSummary : GenericSummary,
            [.. envelope.Errors.Select(error => new ProblemError(error.Code, error.Message, null))]);
    }

    /// <summary>A 400 problem for a request that failed validation, one entry per failure.</summary>
    public static ProblemDetails ToProblemDetails(IEnumerable<ValidationFailure> failures)
    {
        ProblemError[] errors = [.. failures.Select(failure => new ProblemError(
            failure.ErrorCode ?? InvalidRequestCode,
            failure.ErrorMessage,
            ToJsonPointer(failure.PropertyName)))];

        return Create(StatusCodes.Status400BadRequest, ValidationTitle, ValidationSummary, errors);
    }

    /// <summary>A 400 problem for a request MVC could not bind, one entry per model error.</summary>
    public static ProblemDetails ToProblemDetails(ModelStateDictionary modelState)
    {
        List<ProblemError> errors = [];
        foreach ((string key, ModelStateEntry? entry) in modelState)
        {
            foreach (ModelError error in entry?.Errors ?? [])
            {
                // An empty message means the error carries an exception, whose text must not reach the caller.
                string detail = string.IsNullOrEmpty(error.ErrorMessage) ? "The input was not valid." : error.ErrorMessage;
                errors.Add(new ProblemError(InvalidRequestCode, detail, ToJsonPointer(key)));
            }
        }

        return Create(StatusCodes.Status400BadRequest, ValidationTitle, ValidationSummary, [.. errors]);
    }

    /// <summary>A problem for a status that has no <see cref="ErrorType"/>, with the default entry for that status.</summary>
    public static ProblemDetails ToProblemDetails(int status)
    {
        return Create(status, null, GenericSummary, DefaultError(status) is { } error ? [error] : []);
    }

    /// <summary>A problem for a status that has no <see cref="ErrorType"/>, such as a failed upstream call.</summary>
    public static ProblemDetails ToProblemDetails(int status, ProblemError error)
    {
        return Create(status, null, GenericSummary, [error]);
    }

    /// <summary>
    /// Registered as <c>ProblemDetailsOptions.CustomizeProblemDetails</c>. Gives every problem an <c>instance</c>,
    /// the category <c>type</c> for its status, and a default <c>errors</c> entry when it has none, so problems
    /// the framework builds look like the ones built here.
    /// </summary>
    public static void Customize(ProblemDetailsContext context)
    {
        ProblemDetails problem = context.ProblemDetails;
        HttpContext httpContext = context.HttpContext;
        int status = problem.Status ?? httpContext.Response.StatusCode;

        if (problem.Instance is null && httpContext.Request.Path.HasValue)
        {
            problem.Instance = httpContext.Request.Path.Value;
        }

        if (s_categoryTypes.TryGetValue(status, out string? type))
        {
            problem.Type = type;
        }

        if (!problem.Extensions.ContainsKey(ErrorsKey) && DefaultError(status) is { } error)
        {
            problem.Extensions[ErrorsKey] = new[] { error };
        }
    }

    /// <summary>
    /// The value the framework puts in a problem's <c>traceId</c>: the W3C trace id when there is one.
    /// </summary>
    public static string GetTraceId(HttpContext httpContext)
    {
        return Activity.Current?.Id ?? httpContext.TraceIdentifier;
    }

    /// <summary>
    /// Builds the response with MVC's <see cref="ProblemDetailsFactory"/>, so framework defaults, <c>traceId</c>,
    /// and <see cref="Customize"/> apply as they do for problems MVC creates itself.
    /// </summary>
    public static ObjectResult ToObjectResult(this ProblemDetails problem, HttpContext httpContext)
    {
        var factory = httpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        ProblemDetails built = factory.CreateProblemDetails(
            httpContext,
            problem.Status,
            problem.Title,
            problem.Type,
            problem.Detail,
            problem.Instance);

        // The factory has already added a default entry. The authored errors replace it.
        foreach ((string key, object? value) in problem.Extensions)
        {
            built.Extensions[key] = value;
        }

        return new ObjectResult(built)
        {
            StatusCode = built.Status,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static ProblemDetails Create(int status, string? title, string summary, ProblemError[] errors)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = errors.Length == 1 ? errors[0].Detail : summary
        };

        if (s_categoryTypes.TryGetValue(status, out string? type))
        {
            problem.Type = type;
        }

        if (errors.Length > 0)
        {
            problem.Extensions[ErrorsKey] = errors;
        }

        return problem;
    }

    private static ProblemError? DefaultError(int status)
    {
        return status switch
        {
            StatusCodes.Status401Unauthorized => new("Unauthorized", "Authentication is required to access this resource.", null),
            StatusCodes.Status403Forbidden => new("Forbidden", "You do not have permission to access this resource.", null),
            StatusCodes.Status404NotFound => new("NotFound", "The requested resource was not found.", null),
            StatusCodes.Status400BadRequest => new(InvalidRequestCode, InvalidRequestDetail, null),
            >= 400 and < 500 => new(InvalidRequestCode, StatusTitle(status), null),
            >= 500 => new("Unexpected", "An unexpected error occurred.", null),
            _ => null
        };
    }

    private static string StatusTitle(int status)
    {
        string reason = ReasonPhrases.GetReasonPhrase(status);
        return string.IsNullOrEmpty(reason) ? InvalidRequestDetail : reason;
    }

    // "Items[0].Name" becomes "#/items/0/name". Model-state paths from System.Text.Json start with "$".
    private static string ToJsonPointer(string? path)
    {
        var pointer = new StringBuilder("#");
        foreach (string segment in (path ?? string.Empty).TrimStart('$').Split(s_pathSeparators, StringSplitOptions.RemoveEmptyEntries))
        {
            string name = segment.All(char.IsAsciiDigit) ? segment : JsonNamingPolicy.CamelCase.ConvertName(segment);
            pointer.Append('/').Append(name.Replace("~", "~0").Replace("/", "~1"));
        }

        return pointer.ToString();
    }

    private static Dictionary<int, string> BuildCategoryTypes()
    {
        Dictionary<int, string> types = [];
        foreach (ErrorType errorType in Enum.GetValues<ErrorType>())
        {
            types.TryAdd(
                errorType.ToHttpStatusCode(),
                TypeBase + JsonNamingPolicy.KebabCaseLower.ConvertName(errorType.ToString()));
        }

        return types;
    }
}
