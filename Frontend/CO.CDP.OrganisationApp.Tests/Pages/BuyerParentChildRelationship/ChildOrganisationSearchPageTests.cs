using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationSearchPageTests
{
    private readonly ChildOrganisationSearchPage _model;

    public ChildOrganisationSearchPageTests()
    {
        var organisationClientMock = new Mock<IOrganisationClient>();
        _model = new ChildOrganisationSearchPage(organisationClientMock.Object);
    }

    [Fact]
    public void OnGet_ReturnsPageResult()
    {
        _model.Id = Guid.NewGuid();

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }
}
