using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.FeatureManagement;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class SignedOutModelTest
{
    private readonly Mock<IFeatureManager> featureManagerMock = new();
    private readonly Mock<IFtsUrlService> ftsUrlServiceMock = new();

    [Fact]
    public async Task OnGet_WhenFtsRedirectEnabled_HomePageLinkShouldBeFtsHomePage()
    {
        featureManagerMock.Setup(f => f.IsEnabledAsync("AllowFtsRedirectLinks")).ReturnsAsync(true);
        ftsUrlServiceMock.Setup(f => f.BuildUrl("", null, null)).Returns("http://fts-example.com/");
        var model = new SignedOutModel(featureManagerMock.Object, ftsUrlServiceMock.Object);

        await model.OnGet();

        model.HomePageLink.Should().Be("http://fts-example.com/");
    }

    [Fact]
    public async Task OnGet_WhenFtsRedirectDisabled_HomePageLinkShouldBeSirsiHomePage()
    {
        featureManagerMock.Setup(f => f.IsEnabledAsync("AllowFtsRedirectLinks")).ReturnsAsync(false);
        var model = new SignedOutModel(featureManagerMock.Object, ftsUrlServiceMock.Object);

        await model.OnGet();

        model.HomePageLink.Should().Be("/");
    }
}
