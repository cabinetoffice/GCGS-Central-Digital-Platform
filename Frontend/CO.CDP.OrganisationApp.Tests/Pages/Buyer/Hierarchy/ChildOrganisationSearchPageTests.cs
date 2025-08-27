using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.FeatureManagement;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationSearchPageTests
{
    private readonly Mock<IFeatureManager> _featureManager;
    private readonly ChildOrganisationSearchPage _page;

    public ChildOrganisationSearchPageTests()
    {
        _featureManager = new Mock<IFeatureManager>();
        _page = new ChildOrganisationSearchPage(_featureManager.Object);
    }

    [Fact]
    public async Task OnGet_ReturnsPageResult()
    {
        _page.Id = Guid.NewGuid();

        var result = await _page.OnGetAsync();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_SearchRegistryPponEnabled_SetsSearchRegistryPponEnabledToTrue()
    {
        _featureManager.Setup(f => f.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(true);

        await _page.OnGetAsync();

        _page.SearchRegistryPponEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task OnGet_SearchRegistryPponDisabled_SetsSearchRegistryPponEnabledToFalse()
    {
        _featureManager.Setup(f => f.IsEnabledAsync(FeatureFlags.SearchRegistryPpon)).ReturnsAsync(false);

        await _page.OnGetAsync();

        _page.SearchRegistryPponEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task OnPost_EmptyQuery_ReturnsBadRequest()
    {
        _page.Query = string.Empty;

        var result = await _page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        _page.ModelState.IsValid.Should().BeFalse();
        _page.ModelState.Should().ContainKey("Query");
        _page.ModelState["Query"]!.Errors.Should().HaveCount(1);
        _page.ModelState["Query"]!.Errors[0].ErrorMessage.Should().Be(StaticTextResource.Global_EnterSearchTerm);
    }


    [Fact]
    public async Task OnPost_QueryWithInvalidCharacters_ReturnsBadRequest()
    {
        _page.Query = "@#$%^&*()"; // Invalid characters

        var result = await _page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        _page.ModelState.IsValid.Should().BeFalse();
        _page.ModelState.Should().ContainKey("Query");
        _page.ModelState["Query"]!.Errors.Should().HaveCount(1);
        _page.ModelState["Query"]!.Errors[0].ErrorMessage.Should().Be(StaticTextResource.PponSearch_Invalid_Search_Value);
    }

    [Fact]
    public async Task OnPost_ValidQueryUsesCleanedText_RedirectsWithCleanedQuery()
    {
        var id = Guid.NewGuid();
        var originalQuery = "  Test Organisation  "; // Query with leading/trailing whitespace
        var expectedCleanedQuery = "Test Organisation";

        _page.Id = id;
        _page.Query = originalQuery;

        var result = await _page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("ChildOrganisationResultsPage");
        redirectResult.RouteValues.Should().ContainKey("Id").And.ContainValue(id);
        redirectResult.RouteValues.Should().ContainKey("query").And.ContainValue(expectedCleanedQuery);
    }

    [Fact]
    public async Task OnPost_ValidQuery_RedirectsToResultsPage()
    {
        var id = Guid.NewGuid();
        var query = "Test Organisation";

        _page.Id = id;
        _page.Query = query;

        var result = await _page.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("ChildOrganisationResultsPage");
        redirectResult.RouteValues.Should().ContainKey("Id");
        redirectResult.RouteValues.Should().ContainKey("query");
        redirectResult.RouteValues.Should().ContainKey("Id").And.ContainValue(id);
        redirectResult.RouteValues.Should().ContainKey("query").And.ContainValue(query);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void ValidateSearchInput_WithNullOrEmptyInput_ReturnsGlobalEnterSearchTermError(string? searchText)
    {
        var result = ChildOrganisationSearchPage.ValidateSearchInput(searchText ?? string.Empty);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(StaticTextResource.Global_EnterSearchTerm);
        result.CleanedSearchText.Should().Be(string.Empty);
    }

    [Theory]
    [InlineData("ab", "ab")]
    [InlineData("Test Organisation", "Test Organisation")]
    [InlineData("Company123", "Company123")]
    [InlineData("Test-Company", "Test-Company")]
    [InlineData("Multi Word Company", "Multi Word Company")]
    [InlineData("UPPERCASE", "UPPERCASE")]
    [InlineData("lowercase", "lowercase")]
    [InlineData("123456789", "123456789")]
    [InlineData("Test 123", "Test 123")]
    public void ValidateSearchInput_WithValidInput_ReturnsValidResult(string searchText, string expectedCleanedText)
    {
        var result = ChildOrganisationSearchPage.ValidateSearchInput(searchText);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().Be(string.Empty);
        result.CleanedSearchText.Should().Be(expectedCleanedText);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    public void ValidateSearchInput_WithSingleAndDoubleCharacter_ReturnsValidResult(string searchText)
    {
        var result = ChildOrganisationSearchPage.ValidateSearchInput(searchText);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().Be(string.Empty);
        result.CleanedSearchText.Should().Be(searchText);
    }

    [Theory]
    [InlineData("  test  ", "test")]
    [InlineData("\ttest\t", "test")]
    [InlineData("  test company  ", "test company")]
    public void ValidateSearchInput_WithWhitespaceAroundValidInput_TrimsAndReturnsValidResult(string searchText, string expectedCleanedText)
    {
        var result = ChildOrganisationSearchPage.ValidateSearchInput(searchText);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().Be(string.Empty);
        result.CleanedSearchText.Should().Be(expectedCleanedText);
    }
}
