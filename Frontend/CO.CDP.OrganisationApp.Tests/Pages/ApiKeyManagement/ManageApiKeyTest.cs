using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class ManageApiKeyTest
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient = new();
    private readonly ManageApiKeyModel _model;

    public ManageApiKeyTest()
    {
        _model = new ManageApiKeyModel(_mockOrganisationClient.Object);
        _model.Id = Guid.NewGuid();
    }

    [Fact]
    public async Task OnGet_WhenAuthenticationKeysAreRetrieved_ShouldReturnPage()
    {
        var authenticationKeys = new List<AuthenticationKey> {
            new AuthenticationKey(createdOn: DateTimeOffset.UtcNow.AddDays(-1), name: "TestKey1", revoked: false, updatedOn: DateTimeOffset.UtcNow)
        };

        _mockOrganisationClient
            .Setup(client => client.GetAuthenticationKeysAsync(It.IsAny<Guid>()))
            .ReturnsAsync(authenticationKeys);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();

        _model.AuthenticationKeys.Should().HaveCount(1);
        _model.AuthenticationKeys.FirstOrDefault()!.Name.Should().Be("TestKey1");
    }

    [Fact]
    public async Task OnGet_WhenApiExceptionOccurs_ShouldRedirectToPageNotFound()
    {
        _mockOrganisationClient.Setup(client => client.GetAuthenticationKeysAsync(It.IsAny<Guid>()))
                              .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public void OnPost_ShouldRedirectToCreateApiKeyPage()
    {
        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("CreateApiKey");

        result.As<RedirectToPageResult>().RouteValues!["Id"].Should().Be(_model.Id);
    }
}