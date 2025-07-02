using System.ComponentModel.DataAnnotations;
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

        var validationContext = new ValidationContext(_page);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(_page, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            foreach (var memberName in validationResult.MemberNames)
            {
                if (validationResult.ErrorMessage != null)
                    _page.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
            }
        }

        var result = await _page.OnPostAsync();

        result.Should().BeOfType<PageResult>();
        _page.ModelState.IsValid.Should().BeFalse();
        _page.ModelState.Should().ContainKey("Query");
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
        redirectResult.RouteValues["Id"].Should().Be(id);
        redirectResult.RouteValues["query"].Should().Be(query);
    }
}
