using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;
public static class OrganisationEntityFactory
{
    public static Organisation.WebApiClient.Organisation GivenClientModel()
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, Guid.NewGuid(), null, "Test Org", []);
    }

    public static ProblemDetails GivenProblemDetails(
        string anOrganisationWithThisNameAlreadyExists = "An organisation with this name already exists.",
        string duplicateOrganisation = "Duplicate organisation",
        int statusCode = 500,
        string code = "UNKNOWN_CODE"
    )
    {
        var problemDetails = new ProblemDetails(
            title: duplicateOrganisation,
            detail: anOrganisationWithThisNameAlreadyExists,
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
        return problemDetails;
    }

    public static ApiException<ProblemDetails> GivenApiException(
        int statusCode = 500,
        ProblemDetails? problemDetails = null
    )
    {
        var aex = new ApiException<ProblemDetails>(
            "Duplicate organisation",
            statusCode,
            "Bad Request",
            null,
            problemDetails ?? new ProblemDetails("Detail", "Instance", statusCode,
                "Problem title", "Problem type"),
            null
        );
        return aex;
    }
}