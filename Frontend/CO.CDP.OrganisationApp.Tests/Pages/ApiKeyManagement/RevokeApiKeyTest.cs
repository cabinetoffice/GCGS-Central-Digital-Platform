using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class RevokeApiKeyTest
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly RevokeApiKeyModel _pageModel;

    public RevokeApiKeyTest()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _pageModel = new RevokeApiKeyModel(_mockOrganisationClient.Object)
        {
            Id = Guid.NewGuid(),
            ApiKeyName = "TestApiKey"
        };
    }

    [Fact]
    public async Task OnPost_ValidModelState_ShouldRedirectToManageApiKeyPage()
    {
        _mockOrganisationClient
            .Setup(client => client.RevokeAuthenticationKeyAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ManageApiKey");
        result.As<RedirectToPageResult>().RouteValues!["Id"].Should().Be(_pageModel.Id);

        _mockOrganisationClient.Verify(client => client.RevokeAuthenticationKeyAsync(_pageModel.Id, _pageModel.ApiKeyName), Times.Once);
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ShouldReturnPageResult()
    {
        _pageModel.ModelState.AddModelError("ApiKeyName", "ApiKeyName is required");

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<PageResult>();

        _mockOrganisationClient.Verify(client => client.RevokeAuthenticationKeyAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_ApiException404_ShouldRedirectToPageNotFound()
    {
        _mockOrganisationClient.Setup(client => client.RevokeAuthenticationKeyAsync(It.IsAny<Guid>(), It.IsAny<string>()))
                               .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");

        _mockOrganisationClient.Verify(client => client.RevokeAuthenticationKeyAsync(_pageModel.Id, _pageModel.ApiKeyName), Times.Once);
    }
}