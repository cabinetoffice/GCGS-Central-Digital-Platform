using CO.CDP.EntityVerification.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using static CO.CDP.EntityVerification.UseCase.LookupIdentifierUseCase.LookupIdentifierException;

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

    [Fact]
    public void ItMapsInvalidIdentifierFormatExceptionToInvalidIdentifierError()
    {
        var exception = new InvalidIdentifierFormatException("Invalid identifier");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("INVALID_IDENTIFIER_FORMAT", result.error);
    }
}