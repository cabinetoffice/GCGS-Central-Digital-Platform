using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;

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
}
