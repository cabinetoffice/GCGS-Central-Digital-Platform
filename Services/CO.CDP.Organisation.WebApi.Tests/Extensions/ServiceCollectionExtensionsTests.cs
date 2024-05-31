using System.Reflection;
using Microsoft.AspNetCore.Http;
using static CO.CDP.Organisation.WebApi.UseCase.RegisterOrganisationUseCase.RegisterOrganisationException;
using static CO.CDP.OrganisationInformation.Persistence.IOrganisationRepository.OrganisationRepositoryException;
using static CO.CDP.OrganisationInformation.Persistence.ITenantRepository.TenantRepositoryException;

namespace CO.CDP.Organisation.WebApi.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    private static (int status, string error) InvokeMapException(Exception exception)
    {
        var methodInfo = typeof(CO.CDP.Organisation.WebApi.Extensions.ServiceCollectionExtensions)
            .GetMethod("MapException", BindingFlags.NonPublic | BindingFlags.Static);
        return ((int status, string error))methodInfo.Invoke(null, new object[] { exception });
    }

  [Fact]
        public void MapException_Should_Return_UnprocessableEntity_For_BadHttpRequestException()
        {
            var exception = new BadHttpRequestException("Bad request");
            var result = InvokeMapException(exception);

            Assert.Equal(StatusCodes.Status422UnprocessableEntity, result.status);
            Assert.Equal("UNPROCESSABLE_ENTITY", result.error);
        }

        [Fact]
        public void MapException_Should_Return_NotFound_For_DuplicateOrganisationException()
        {
            var exception = new DuplicateOrganisationException("Duplicate organisation");
            var result = InvokeMapException(exception);

            Assert.Equal(StatusCodes.Status404NotFound, result.status);
            Assert.Equal("ORGANISATION_ALREADY_EXISTS", result.error);
        }

        [Fact]
        public void MapException_Should_Return_NotFound_For_UnknownPersonException()
        {
            var exception = new UnknownPersonException("Unknown person");
            var result = InvokeMapException(exception);

            Assert.Equal(StatusCodes.Status404NotFound, result.status);
            Assert.Equal("PERSON_DOES_NOT_EXIST", result.error);
        }

        [Fact]
        public void MapException_Should_Return_BadRequest_For_ArgumentNullException()
        {
            var exception = new ArgumentNullException();
            var result = InvokeMapException(exception);

            Assert.Equal(StatusCodes.Status400BadRequest, result.status);
            Assert.Equal("ARGUMENT_NULL", result.error);
        }

        [Fact]
        public void MapException_Should_Return_BadRequest_For_InvalidOperationException()
        {
            var exception = new InvalidOperationException();
            var result = InvokeMapException(exception);

            Assert.Equal(StatusCodes.Status400BadRequest, result.status);
            Assert.Equal("INVALID_OPERATION", result.error);
        }

        [Fact]
        public void MapException_Should_Return_InternalServerError_For_GenericException()
        {
            var exception = new Exception();
            var result = InvokeMapException(exception);

            Assert.Equal(StatusCodes.Status500InternalServerError, result.status);
            Assert.Equal("GENERIC_ERROR", result.error);
        }
}
