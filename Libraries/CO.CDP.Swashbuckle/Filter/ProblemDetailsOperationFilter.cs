using System.Net.Mime;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Swashbuckle.Filter;

/// Inspired by the <a href="https://stackoverflow.com/a/75673373">StackOverflow answer</a>.
public class ProblemDetailsOperationFilter(Dictionary<string, List<string>> errorCodes) : IOperationFilter
{
    public ProblemDetailsOperationFilter() : this([])
    {
    }

    private static OpenApiObject Status400ProblemDetails(string code) => CreateProblemDetails(
       "https://tools.ietf.org/html/rfc7231#section-6.5.1",
       ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest),
       StatusCodes.Status400BadRequest,
       code
   );

    private static OpenApiObject Status401ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7235#section-3.1",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized),
        StatusCodes.Status401Unauthorized,
        code
    );

    private static OpenApiObject Status403ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden),
        StatusCodes.Status403Forbidden,
        code
    );

    private static OpenApiObject Status404ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound),
        StatusCodes.Status404NotFound,
        code
    );

    private static OpenApiObject Status406ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7231#section-6.5.6",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status406NotAcceptable),
        StatusCodes.Status406NotAcceptable,
        code
    );

    private static OpenApiObject Status409ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status409Conflict),
        StatusCodes.Status409Conflict,
        code
    );

    private static OpenApiObject Status415ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7231#section-6.5.13",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status415UnsupportedMediaType),
        StatusCodes.Status415UnsupportedMediaType,
        code,
        "Error details."
    );

    private static OpenApiObject Status422ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc4918#section-11.2",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status422UnprocessableEntity),
        StatusCodes.Status422UnprocessableEntity,
        code
    );

    private static OpenApiObject Status500ProblemDetails(string code) => CreateProblemDetails(
        "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError),
        StatusCodes.Status500InternalServerError,
        code
    );

    private readonly Dictionary<string, Func<string, OpenApiObject>> _problemDetails = new()
    {
        { StatusCodes.Status400BadRequest.ToString(), Status400ProblemDetails },
        { StatusCodes.Status401Unauthorized.ToString(), Status401ProblemDetails },
        { StatusCodes.Status403Forbidden.ToString(), Status403ProblemDetails },
        { StatusCodes.Status404NotFound.ToString(), Status404ProblemDetails },
        { StatusCodes.Status406NotAcceptable.ToString(), Status406ProblemDetails },
        { StatusCodes.Status409Conflict.ToString(), Status409ProblemDetails },
        { StatusCodes.Status415UnsupportedMediaType.ToString(), Status415ProblemDetails },
        { StatusCodes.Status422UnprocessableEntity.ToString(), Status422ProblemDetails },
        { StatusCodes.Status500InternalServerError.ToString(), Status500ProblemDetails },
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var operationResponse in operation.Responses)
        {
            _problemDetails.TryGetValue(operationResponse.Key, out var problemDetail);
            UpdateExamples(operationResponse, problemDetail);
        }
    }

    private void UpdateExamples(
        KeyValuePair<string, OpenApiResponse> operationResponse,
        Func<string, OpenApiObject>? problemDetail)
    {
        foreach (var content in operationResponse.Value.Content)
        {
            if (errorCodes.TryGetValue(operationResponse.Key, out var codes))
            {
                content.Value.Examples = codes.Select(code => (code, new OpenApiExample
                {
                    Summary = code,
                    Value = problemDetail?.Invoke(code)
                })).ToDictionary();
            }
            else
            {
                content.Value.Example = problemDetail?.Invoke("");
            }
        }
    }

    private static OpenApiObject CreateProblemDetails(string type, string title, int status, string code = "", string detail = "")
    {
        var problemDetails = new OpenApiObject
        {
            ["type"] = new OpenApiString(type),
            ["title"] = new OpenApiString(title),
            ["status"] = new OpenApiInteger(status)
        };

        if (!string.IsNullOrEmpty(code))
        {
            problemDetails["code"] = new OpenApiString(code);
        }

        if (!string.IsNullOrEmpty(detail))
        {
            problemDetails["detail"] = new OpenApiString(detail);
        }

        return problemDetails;
    }
}