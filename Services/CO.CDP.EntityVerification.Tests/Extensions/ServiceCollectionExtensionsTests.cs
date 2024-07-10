using CO.CDP.EntityVerification.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;

namespace CO.CDP.EntityVerification.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ItMapsBadHttpRequestExceptionToUnprocessableEntityError()
    {
        var exception = new BadHttpRequestException("Bad request");
        var result = ServiceCollectionExtensions.MapException(exception);

        result.status.Should().Be(StatusCodes.Status422UnprocessableEntity);
        result.error.Should().Be("UNPROCESSABLE_ENTITY");
    }
}