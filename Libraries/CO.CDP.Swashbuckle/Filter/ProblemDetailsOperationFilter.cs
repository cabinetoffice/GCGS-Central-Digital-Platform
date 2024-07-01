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

    private static OpenApiObject Status400ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest)),
        ["status"] = new OpenApiInteger(StatusCodes.Status400BadRequest),
        ["detail"] = new OpenApiString("Error details."),
        ["code"] = new OpenApiString(code),
        ["errors"] = new OpenApiObject
        {
            ["property1"] = new OpenApiArray
            {
                new OpenApiString("The property field is required"),
            },
        },
    };

    private static OpenApiObject Status401ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7235#section-3.1"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized)),
        ["status"] = new OpenApiInteger(StatusCodes.Status401Unauthorized),
        ["detail"] = new OpenApiString("Error details."),
        ["code"] = new OpenApiString(code),
    };

    private static OpenApiObject Status403ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.3"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden)),
        ["status"] = new OpenApiInteger(StatusCodes.Status403Forbidden),
        ["detail"] = new OpenApiString("Error details."),
        ["code"] = new OpenApiString(code),
    };

    private static OpenApiObject Status404ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.4"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound)),
        ["status"] = new OpenApiInteger(StatusCodes.Status404NotFound),
        ["detail"] = new OpenApiString("Error details."),
        ["code"] = new OpenApiString(code),
    };

    private static OpenApiObject Status406ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.6"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status406NotAcceptable)),
        ["status"] = new OpenApiInteger(StatusCodes.Status406NotAcceptable),
        ["detail"] = new OpenApiString("Error details."),
    };

    private static OpenApiObject Status409ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.8"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status409Conflict)),
        ["status"] = new OpenApiInteger(StatusCodes.Status409Conflict),
        ["detail"] = new OpenApiString("Error details."),
    };

    private static OpenApiObject Status415ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.13"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status415UnsupportedMediaType)),
        ["status"] = new OpenApiInteger(StatusCodes.Status415UnsupportedMediaType),
        ["detail"] = new OpenApiString("Error details."),
    };

    private static OpenApiObject Status422ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status422UnprocessableEntity)),
        ["status"] = new OpenApiInteger(StatusCodes.Status422UnprocessableEntity),
        ["detail"] = new OpenApiString("Error details."),
    };

    private static OpenApiObject Status500ProblemDetails(string code) => new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError)),
        ["status"] = new OpenApiInteger(StatusCodes.Status500InternalServerError),
        ["detail"] = new OpenApiString("Error details."),
    };

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
                foreach (var code in codes)
                {
                    content.Value.Examples.Add(code, new OpenApiExample
                    {
                        Summary = code,
                        Value = problemDetail?.Invoke(code)
                    });
                }
            }
            else
            {
                content.Value.Example = problemDetail?.Invoke("");
            }
        }
    }
}