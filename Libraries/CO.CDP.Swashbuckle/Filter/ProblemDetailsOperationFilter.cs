using System.Net.Mime;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Swashbuckle.Filter;

/// Inspired by the <a href="https://stackoverflow.com/a/75673373">StackOverflow answer</a>.
public class ProblemDetailsOperationFilter : IOperationFilter
{
    private static readonly OpenApiObject Status400ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status400BadRequest)),
        ["status"] = new OpenApiInteger(StatusCodes.Status400BadRequest),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
        ["errors"] = new OpenApiObject
        {
            ["property1"] = new OpenApiArray
            {
                new OpenApiString("The property field is required"),
            },
        },
    };

    private static readonly OpenApiObject Status401ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7235#section-3.1"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized)),
        ["status"] = new OpenApiInteger(StatusCodes.Status401Unauthorized),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject Status403ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.3"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden)),
        ["status"] = new OpenApiInteger(StatusCodes.Status403Forbidden),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject Status404ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.4"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound)),
        ["status"] = new OpenApiInteger(StatusCodes.Status404NotFound),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject Status406ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.6"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status406NotAcceptable)),
        ["status"] = new OpenApiInteger(StatusCodes.Status406NotAcceptable),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject Status409ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.8"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status409Conflict)),
        ["status"] = new OpenApiInteger(StatusCodes.Status409Conflict),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject Status415ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.13"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status415UnsupportedMediaType)),
        ["status"] = new OpenApiInteger(StatusCodes.Status415UnsupportedMediaType),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject _status422ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status422UnprocessableEntity)),
        ["status"] = new OpenApiInteger(StatusCodes.Status422UnprocessableEntity),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private static readonly OpenApiObject Status500ProblemDetails = new()
    {
        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.6.1"),
        ["title"] = new OpenApiString(ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError)),
        ["status"] = new OpenApiInteger(StatusCodes.Status500InternalServerError),
        ["traceId"] = new OpenApiString("00-982607166a542147b435be3a847aaa12-fc75498eb9f09a83-00"),
    };

    private readonly Dictionary<string, IOpenApiAny> _problemDetails = new()
    {
        { StatusCodes.Status400BadRequest.ToString(), Status400ProblemDetails },
        { StatusCodes.Status401Unauthorized.ToString(), Status401ProblemDetails },
        { StatusCodes.Status403Forbidden.ToString(), Status403ProblemDetails },
        { StatusCodes.Status404NotFound.ToString(), Status404ProblemDetails },
        { StatusCodes.Status406NotAcceptable.ToString(), Status406ProblemDetails },
        { StatusCodes.Status409Conflict.ToString(), Status409ProblemDetails },
        { StatusCodes.Status415UnsupportedMediaType.ToString(), Status415ProblemDetails },
        { StatusCodes.Status422UnprocessableEntity.ToString(), _status422ProblemDetails },
        { StatusCodes.Status500InternalServerError.ToString(), Status500ProblemDetails },
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var operationResponse in operation.Responses)
        {
            _problemDetails.TryGetValue(operationResponse.Key, out var problemDetail);
            foreach (var content in operationResponse.Value.Content)
            {
                content.Value.Example = problemDetail;
            }
        }
    }
}