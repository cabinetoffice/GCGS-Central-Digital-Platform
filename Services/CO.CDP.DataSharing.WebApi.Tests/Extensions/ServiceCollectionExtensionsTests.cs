using Microsoft.AspNetCore.Http;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;

namespace DataSharing.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void MapException_Should_Return_InternalServerError_For_SharedConsentNotFoundException()
    {
        var exception = new SharedConsentNotFoundException("Shared Consent not found.");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status500InternalServerError, result.status);
        Assert.Equal("SHARED_CONSENT_NOT_FOUND", result.error);
    }
}