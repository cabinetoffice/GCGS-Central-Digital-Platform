using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationResultsPageTests
{
    private readonly ChildOrganisationResultsPage _model = new();

    [Fact]
    public void OnGet_SetsPropertiesFromQuery()
    {
        var id = Guid.NewGuid();
        const string query = "test";

        _model.Id = id;
        _model.Query = query;

        _model.OnGet();

        _model.Id.Should().Be(id);
        _model.Query.Should().Be(query);
    }

    [Fact]
    public void OnGet_WithNonEmptyQuery_PopulatesResults()
    {
        _model.Query = "test query";

        _model.OnGet();

        _model.Results.Should().NotBeEmpty();
        _model.Results.Should().HaveCount(3);
    }

    [Fact]
    public void OnGet_WithEmptyQuery_ReturnsEmptyResults()
    {
        _model.Query = string.Empty;

        _model.OnGet();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public void OnGet_WithNullQuery_ReturnsEmptyResults()
    {
        _model.Query = null;

        _model.OnGet();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public void OnGet_WithWhitespaceQuery_ReturnsEmptyResults()
    {
        _model.Query = "   ";

        _model.OnGet();

        _model.Results.Should().BeEmpty();
    }

    [Fact]
    public void OnPost_WithInvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("Test", "Test error");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WithValidModelState_ReturnsPageResult()
    {
        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void Results_DefaultsToEmptyList()
    {
        var model = new ChildOrganisationResultsPage();

        model.Results.Should().NotBeNull();
        model.Results.Should().BeEmpty();
    }

    [Fact]
    public void SelectedOrganisationId_DefaultsToNull()
    {
        var model = new ChildOrganisationResultsPage();

        model.SelectedOrganisationId.Should().BeNull();
    }

    [Fact]
    public void OnGet_ResultsContainExpectedOrganisationData()
    {
        _model.Query = "test query";

        _model.OnGet();

        _model.Results.Should().AllBeOfType<OrganisationDto>();
        _model.Results.Should().Contain(o => o.Name == "Stark Industries");
        _model.Results.Should().Contain(o => o.Name == "Wayne Enterprises");
        _model.Results.Should().Contain(o => o.Name == "Oscorp");

        _model.Results.Should().OnlyContain(o =>
            o.Roles.Contains(PartyRole.Buyer) &&
            o.Roles.Count == 1);

        var starkIndustries = _model.Results.FirstOrDefault(o => o.Name == "Stark Industries");
        starkIndustries?.Identifiers.Should().Contain("DUNS: 123456789");
    }
}
