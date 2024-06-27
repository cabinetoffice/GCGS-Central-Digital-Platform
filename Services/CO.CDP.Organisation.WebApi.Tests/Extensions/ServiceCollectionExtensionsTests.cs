using CO.CDP.Organisation.WebApi.Extensions;
using CO.CDP.Organisation.WebApi.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using static CO.CDP.Organisation.WebApi.UseCase.RegisterOrganisationUseCase.RegisterOrganisationException;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;

namespace CO.CDP.Organisation.WebApi.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void MapException_Should_Return_UnprocessableEntity_For_BadHttpRequestException()
    {
        var exception = new BadHttpRequestException("Bad request");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.status);
        Assert.Equal("UNPROCESSABLE_ENTITY", result.error);
    }

    [Fact]
    public void MapException_Should_Return_NotFound_For_DuplicateOrganisationException()
    {
        var exception = new DuplicateOrganisationException("Duplicate organisation");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("ORGANISATION_ALREADY_EXISTS", result.error);
    }

    [Fact]
    public void MapException_Should_Return_NotFound_For_UnknownPersonException()
    {
        var exception = new UnknownPersonException("Unknown person");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status404NotFound, result.status);
        Assert.Equal("PERSON_DOES_NOT_EXIST", result.error);
    }

    [Fact]
    public void MapException_Should_Return_BadRequest_For_ArgumentNullException()
    {
        var exception = new ArgumentNullException();
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("ARGUMENT_NULL", result.error);
    }

    [Fact]
    public void MapException_Should_Return_BadRequest_For_InvalidOperationException()
    {
        var exception = new InvalidOperationException();
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("INVALID_OPERATION", result.error);
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
    public void MapException_Should_Return_BadRequest_For_InvalidQueryException()
    {
        var exception = new InvalidQueryException("Missing query parameters");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("MISSING_QUERY_PARAMETERS", result.error);
    }

    [Fact]
    public void ErrorCodes_ShouldReturn_ListOfStatusesMappedToErrorCodes()
    {
        var result = ServiceCollectionExtensions.ErrorCodes();

        result.Should().ContainKey("400");
        result["400"].Should().Contain("ORGANISATION_ALREADY_EXISTS");
        result["400"].Should().Contain("INVALID_BUYER_INFORMATION_UPDATE_ENTITY");
        result["400"].Should().Contain("INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY");
    }
}