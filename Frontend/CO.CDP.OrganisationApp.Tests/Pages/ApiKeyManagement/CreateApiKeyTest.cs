using Amazon.S3;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;
using ProblemDetails = CO.CDP.Organisation.WebApiClient.ProblemDetails;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class CreateApiKeyTest
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient = new();
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly CreateApiKeyModel _model;
    public CreateApiKeyTest()
    {
        _model = new CreateApiKeyModel(_mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsInvalid_ShouldReturnPage()
    {        
        _model.ModelState.AddModelError("key", "error");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WhenApiExceptionOccurs_ShouldRedirectToPageNotFound()
    {        
        _mockOrganisationClient.Setup(client => client.CreateAuthenticationKeyAsync(It.IsAny<Guid>(), It.IsAny<RegisterAuthenticationKey>()))
                              .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        _model.Id = Guid.NewGuid();
        _model.ApiKeyName = "TestApiKey";

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToManageApiKey()
    {
         _model.Id = Guid.NewGuid();

        _mockOrganisationClient
            .Setup(c => c.CreateAuthenticationKeyAsync(_organisationId, DummyApiKeyEntity()));

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        _mockOrganisationClient.Verify(c => c.CreateAuthenticationKeyAsync(It.IsAny<Guid>(), It.IsAny<RegisterAuthenticationKey>()), Times.Once());

        redirectToPageResult.PageName.Should().Be("NewApiKeyDetails");
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenCreateApiKeyModelStateIsInvalid()
    {
        var model = new CreateApiKeyModel(_mockOrganisationClient.Object);

        model.ModelState.AddModelError("ApiKeyName", "Enter the api key name");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Theory]
    [InlineData(ErrorCodes.APIKEY_NAME_ALREADY_EXISTS, ErrorMessagesList.DuplicateApiKeyName, StatusCodes.Status400BadRequest)]
    public async Task OnPost_AddsModelError(string errorCode, string expectedErrorMessage, int statusCode)
    {
        var problemDetails = new ProblemDetails(
            title: errorCode,
            detail: ErrorMessagesList.DuplicateApiKeyName,
            status: statusCode,
            instance: null,
            type: null
        )
        {
            AdditionalProperties =
            {
                { "code", "APIKEY_NAME_ALREADY_EXISTS" }
            }
        };
        var aex = new ApiException<ProblemDetails>(
            "Duplicate Api key name",
            statusCode,
            "Bad Request",
            null,
            problemDetails ?? new ProblemDetails("Detail", "Instance", statusCode, "Problem title", "Problem type"),
            null
        );

        _mockOrganisationClient.Setup(client => client.CreateAuthenticationKeyAsync(It.IsAny<Guid>(), It.IsAny<RegisterAuthenticationKey>()))
            .ThrowsAsync(aex);

        _model.Id = Guid.NewGuid();
        _model.ApiKeyName = "TestApiKey";

        var result = await _model.OnPost();

        _model.ModelState[string.Empty].As<ModelStateEntry>().Errors
            .Should().Contain(e => e.ErrorMessage == expectedErrorMessage);
    }

    private RegisterAuthenticationKey DummyApiKeyEntity()
    {
        return new RegisterAuthenticationKey(key: "_key", name: "name", organisationId: _organisationId);
    }
}