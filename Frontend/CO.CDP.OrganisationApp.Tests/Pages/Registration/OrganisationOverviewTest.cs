using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Registration;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationOverviewTest
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    
    public OrganisationOverviewTest()
    {
        organisationClientMock = new Mock<IOrganisationClient>();        
    }

    [Fact]
    public async Task OnGet_WithValidId_CallsGetOrganisationAsync()
    {
        var id = Guid.NewGuid();        
        var model = GivenOrganisationOverviewModel();
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await model.OnGet(id);

        organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }

    [Fact]
    public async Task OnGet_WithNullId_ThrowsArgumentNullException()
    {
        Guid? id = null;
        var model = GivenOrganisationOverviewModel();

        await Assert.ThrowsAsync<ArgumentNullException>(() => model.OnGet(id));
    }

    private CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CDP.Organisation.WebApiClient.Organisation(null, null, null, id!.Value, null, "Test Org", []);
    }

    private OrganisationOverviewModel GivenOrganisationOverviewModel()
    {
        return new OrganisationOverviewModel(organisationClientMock.Object);
    }
}