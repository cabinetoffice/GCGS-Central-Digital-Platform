
using CO.CDP.Forms.WebApi.Extensions;
using CO.CDP.Forms.WebApi.Model;

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Forms.WebApi.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{

    [Fact]
    public void MapException_Should_Return_NotFound_For_DuplicateOrganisationException()
    {
        var exception = new DuplicateOrganisationException("Duplicate organisation");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("ORGANISATION_ALREADY_EXISTS", result.error);
    }

    [Fact]
    public void MapException_Should_Return_InternalServerError_For_GenericException()
    {
        var exception = new Exception();
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status500InternalServerError, result.status);
        Assert.Equal("GENERIC_ERROR", result.error);
    }

    [Fact]
    public void MapException_Should_Return_NotFound_For_UnknownOrganisationException()
    {
        var exception = new UnknownOrganisationException("Unknown organisation");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status404NotFound, result.status);
        Assert.Equal("UNKNOWN_ORGANISATION", result.error);
    }

    [Fact]
    public void MapException_Should_Return_NotFound_For_UnknownSectionException()
    {
        var exception = new UnknownSectionException("Unknown section");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status404NotFound, result.status);
        Assert.Equal("UNKNOWN_SECTION", result.error);
    }

    [Fact]
    public void MapException_Should_Return_BadRequest_For_UnknownQuestionsException()
    {
        var exception = new UnknownQuestionsException("Unknown questions");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("UNKNOWN_QUESTIONS", result.error);
    }

    [Fact]
    public void ErrorCodes_ShouldReturn_ListOfStatusesMappedToErrorCodes()
    {
        var result = ServiceCollectionExtensions.ErrorCodes();

        result.Should().ContainKey("400");
        result["400"].Should().Contain("UNKNOWN_QUESTIONS");
        result["400"].Should().Contain("ORGANISATION_ALREADY_EXISTS");

        result.Should().ContainKey("404");
        result["404"].Should().Contain("UNKNOWN_ORGANISATION");
        result["404"].Should().Contain("UNKNOWN_SECTION");

    }
}
