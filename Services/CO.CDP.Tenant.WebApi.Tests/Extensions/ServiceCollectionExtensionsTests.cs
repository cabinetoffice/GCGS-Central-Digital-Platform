using System.Reflection;
using CO.CDP.Tenant.WebApi.Extensions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Tenant.WebApi.Tests.Extensions;

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
    public void MapException_Should_Return_NotFound_For_DuplicateTenantException()
    {
        var exception = new DuplicateTenantException("Duplicate tenant");
        var result = ServiceCollectionExtensions.MapException(exception);

        Assert.Equal(StatusCodes.Status400BadRequest, result.status);
        Assert.Equal("TENANT_ALREADY_EXISTS", result.error);
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
    public void ErrorCodes_ShouldReturn_ListOfStatusesMappedToErrorCodes()
    {
        var result = ServiceCollectionExtensions.ErrorCodes();

        result.Should().ContainKey("400");
        result["400"].Should().Contain("TENANT_ALREADY_EXISTS");
        result["400"].Should().Contain("ARGUMENT_NULL");
        result["400"].Should().Contain("INVALID_OPERATION");

        result.Should().ContainKey("422");
        result["422"].Should().Contain("UNPROCESSABLE_ENTITY");
    }

}