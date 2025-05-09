using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using OrganisationType = CO.CDP.Organisation.WebApiClient.OrganisationType;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class ViewAdminsModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly ViewAdminsModel _viewAdminsModel;

    public ViewAdminsModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _viewAdminsModel = new ViewAdminsModel(_mockOrganisationClient.Object);
    }

    [Fact]
    public async Task OnGet_ShouldDisplayAdminUsersOnly()
    {
        var adminPerson = new CO.CDP.Organisation.WebApiClient.Person("john@johnson.com", "John", Guid.NewGuid(), "Johnson", ["ADMIN", "RESPONDER"]);
        var persons = new List<CO.CDP.Organisation.WebApiClient.Person> { adminPerson };

        _mockOrganisationClient
            .Setup(c => c.GetOrganisationPersonsInRoleAsync(_viewAdminsModel.Id, "ADMIN"))
            .ReturnsAsync(persons);

        var result = await _viewAdminsModel.OnGet();

        _viewAdminsModel.Persons.Should().Contain(adminPerson);

        result.Should().BeOfType<PageResult>();
    }
}
