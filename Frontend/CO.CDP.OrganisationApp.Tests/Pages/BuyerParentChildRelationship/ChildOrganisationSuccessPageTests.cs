using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationSuccessPageTests
{
    private readonly ChildOrganisationSuccessPage _model;

    public ChildOrganisationSuccessPageTests()
    {
        var testGuid = Guid.NewGuid();
        _model = new ChildOrganisationSuccessPage
        {
            Id = testGuid,
            OrganisationName = "Test Organisation"
        };
    }

    [Fact]
    public void Constructor_InitializesProperties()
    {
        var testGuid = Guid.NewGuid();
        var model = new ChildOrganisationSuccessPage
        {
            Id = testGuid,
            OrganisationName = "Test Organisation"
        };

        model.Should().NotBeNull();
        model.Id.Should().Be(testGuid);
        model.OrganisationName.Should().Be("Test Organisation");
    }

    [Fact]
    public void OnGet_ReturnsPageResult()
    {
        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void BindProperties_ShouldBeSetCorrectly()
    {
        var expectedId = Guid.NewGuid();
        const string expectedOrgName = "Updated Organisation";

        _model.Id = expectedId;
        _model.OrganisationName = expectedOrgName;

        _model.Id.Should().Be(expectedId);
        _model.OrganisationName.Should().Be(expectedOrgName);
    }
}
