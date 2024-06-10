using CO.CDP.Person.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages;
public static class ProblemDetailsFactory
{
    public static ProblemDetails GivenProblemDetails(
        string detail = "An error occurred.",
        string title = "Error",
        int statusCode = 500,
        string code = "UNKNOWN_CODE"
    )
    {
        return new ProblemDetails(
            title: title,
            detail: detail,
            status: statusCode,
            instance: null,
            type: null
        )
        {
            AdditionalProperties =
            {
                { "code", code }
            }
        };
    }

    public static ApiException<ProblemDetails> GivenApiException(
        int statusCode = 500,
        ProblemDetails? problemDetails = null
    )
    {
        var aex = new ApiException<ProblemDetails>(
            "An error occurred",
            statusCode,
            "Bad Request",
            null,
            problemDetails ?? GivenProblemDetails(statusCode: statusCode),
            null
        );
        return aex;
    }
}