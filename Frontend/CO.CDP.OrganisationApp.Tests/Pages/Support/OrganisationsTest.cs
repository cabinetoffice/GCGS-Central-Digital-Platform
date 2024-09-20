using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp;
using CO.CDP.OrganisationApp.Pages.Support;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class OrganisationsModelTests
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly OrganisationsModel _organisationsModel;

    public OrganisationsModelTests()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock = new Mock<ISession>();
        _organisationsModel = new OrganisationsModel(_organisationClientMock.Object, _sessionMock.Object);
    }

    [Fact]
    public async Task OnGet_ValidType_SetsTitleAndOrganisations_ReturnsPageResult()
    {
        var type = "buyer";
        var approvableOrganisations = new List<ApprovableOrganisation>
        {
            new ApprovableOrganisation (
                approvedById: null,
                approvedByName: "John Smith",
                approvedComment: "",
                approvedOn: null,
                id: new Guid(),
                email: "john@smith.com",
                identifiers: new List<Identifier>(),
                name: "Org 1",
                ppon: "",
                role: ""
                ),
            new ApprovableOrganisation (
                approvedById: null,
                approvedByName: "Smith Johnson",
                approvedComment: "",
                approvedOn: null,
                id: new Guid(),
                email: "smith@johnson.com",
                identifiers: new List<Identifier>(),
                name: "Org 2",
                ppon: "",
                role: ""
            )
        };

        _organisationClientMock.Setup(client => client.GetAllOrganisationsAsync(type, 1000, 0))
            .ReturnsAsync(approvableOrganisations);

        var result = await _organisationsModel.OnGet(type);

        _organisationsModel.Type.Should().Be(type);
        _organisationsModel.Title.Should().Be("Buyer organisations");
        _organisationsModel.Organisations.Should().HaveCount(2);
        result.Should().BeOfType<PageResult>();

        _organisationClientMock.Verify(client => client.GetAllOrganisationsAsync(type, 1000, 0), Times.Once);
    }

    [Fact]
    public async Task OnGet_EmptyOrganisations_ReturnsPageResultWithEmptyList()
    {
        var type = "buyer";
        var approvableOrganisations = new List<ApprovableOrganisation>();

        _organisationClientMock.Setup(client => client.GetAllOrganisationsAsync(type, 1000, 0))
            .ReturnsAsync(approvableOrganisations);

        var result = await _organisationsModel.OnGet(type);

        _organisationsModel.Type.Should().Be(type);
        _organisationsModel.Title.Should().Be("Buyer organisations");
        _organisationsModel.Organisations.Should().BeEmpty();
        result.Should().BeOfType<PageResult>();

        _organisationClientMock.Verify(client => client.GetAllOrganisationsAsync(type, 1000, 0), Times.Once);
    }

    [Fact]
    public async Task OnGet_ClientThrowsException_PropagatesException()
    {
        var type = "supplier";
        _organisationClientMock.Setup(client => client.GetAllOrganisationsAsync(type, 1000, 0))
            .ThrowsAsync(new Exception("API error"));

        Func<Task> act = async () => await _organisationsModel.OnGet(type);

        await act.Should().ThrowAsync<Exception>().WithMessage("API error");

        _organisationClientMock.Verify(client => client.GetAllOrganisationsAsync(type, 1000, 0), Times.Once);
    }
}
