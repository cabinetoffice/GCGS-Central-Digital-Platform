using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationOverviewTest
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly Mock<ISession> sessionMock;

    public OrganisationOverviewTest()
    {
        organisationClientMock = new Mock<IOrganisationClient>();
        sessionMock = new Mock<ISession>();
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

    private Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, id!.Value, null, "Test Org", []);
    }

    private OrganisationOverviewModel GivenOrganisationOverviewModel()
    {
        return new OrganisationOverviewModel(organisationClientMock.Object, sessionMock.Object);
    }
}