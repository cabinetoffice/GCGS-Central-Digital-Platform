using CO.CDP.Organisation.WebApiClient;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;
public static class OrganisationEntityFactory
{
    public static OrganisationWebApiClient.Organisation GivenClientModel()
    {
        return new OrganisationWebApiClient.Organisation(additionalIdentifiers: null, addresses: null, approvedOn: null, contactPoint: null, id: Guid.NewGuid(), identifier: null, name: "Test Org", roles: [], details: new Details(approval: null, pendingRoles: []));
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