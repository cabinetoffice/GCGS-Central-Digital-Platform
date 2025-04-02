using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;
public class IndexModelTests
{
    private readonly Mock<IFeatureManager> _mockFeatureManager;
    private readonly Mock<IConfiguration> _mockConfig;
    private readonly IndexModel _model;

    public IndexModelTests()
    {
        _mockFeatureManager = new Mock<IFeatureManager>();
        _mockConfig = new Mock<IConfiguration>();
        _model = new IndexModel(_mockFeatureManager.Object, _mockConfig.Object);
    }

    [Fact]
    public async Task OnGetAsync_ShouldRedirect_WhenFeatureIsEnabledAndUrlIsSet()
    {
        var expectedUrl = "https://example.com";
        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.RedirectToFtsHomepage))
            .ReturnsAsync(true);

        _mockConfig
            .Setup(x => x["FtsHomepage"])
            .Returns(expectedUrl);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPage_WhenFeatureIsDisabled()
    {
        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.RedirectToFtsHomepage))
            .ReturnsAsync(false);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGetAsync_ShouldReturnPage_WhenFeatureIsEnabledButUrlIsEmpty()
    {
        _mockFeatureManager
            .Setup(x => x.IsEnabledAsync(FeatureFlags.RedirectToFtsHomepage))
            .ReturnsAsync(true);

        _mockConfig
            .Setup(x => x["FtsHomepage"])
            .Returns(string.Empty);

        var result = await _model.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }
}