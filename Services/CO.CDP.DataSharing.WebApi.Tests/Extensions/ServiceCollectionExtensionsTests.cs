using Microsoft.AspNetCore.Http;
using CO.CDP.DataSharing.WebApi.Extensions;
using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void MapException_Should_Return_InternalServerError_For_SharedConsentNotFoundException()
    {
        var exception = new ShareCodeNotFoundException(Constants.ShareCodeNotFoundExceptionMessage);
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status404NotFound, result.status);
        Assert.Equal(Constants.ShareCodeNotFoundExceptionCode, result.error);
    }
}