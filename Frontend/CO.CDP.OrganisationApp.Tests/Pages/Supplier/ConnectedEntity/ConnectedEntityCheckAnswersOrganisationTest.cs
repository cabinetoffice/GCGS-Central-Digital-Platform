using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCheckAnswersOrganisationTest
{
    private readonly ConnectedEntityCheckAnswersOrganisationModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityCheckAnswersOrganisationTest()
    {
        _sessionMock = new Mock<ISession>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new ConnectedEntityCheckAnswersOrganisationModel(_sessionMock.Object, _mockOrganisationClient.Object);
        _model.Id = _organisationId;
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToConnectedEntitySupplierHasControl_WhenModelStateIsInvalid()
    {
        ConnectedEntityState? state = null;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");
    }

    [Fact]
    public async Task OnGet_ShouldReturnPageResult()
    {
        var state = DummyConnectedPersonDetails();
        _model.ConnectedEntityId = _entityId;
        state.OrganisationName = "Org_name";
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToConnectedEntityQuestion_WhenSessionStateIsNull()
    {
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns((ConnectedEntityState?)null);

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _sessionMock
           .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
           .Returns((ConnectedEntityState?)null);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToPageNotFound_WhenRegisterConnectedPersonNotFound()
    {
        var state = DummyConnectedPersonDetails();
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _mockOrganisationClient.Setup(c => c.CreateConnectedEntityAsync(It.IsAny<Guid>(),
            It.IsAny<RegisterConnectedEntity>()))
            .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToConnectedPersonSummary_WhenStateIsValid()
    {
        var state = DummyConnectedPersonDetails();
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedPersonSummary");

    }

    [Fact]
    public async Task OnPost_ShouldRemoveSessionState_WhenModelStateIsValid()
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        await _model.OnPost();

        _sessionMock.Verify(s => s.Remove(Session.ConnectedPersonKey), Times.Once);

    }

    private RegisterConnectedEntity DummyPayload()
    {
        CreateConnectedOrganisation? connectedOrganisation = null;
        CreateConnectedIndividualTrust? connectedIndividualTrust = null;
        var state = DummyConnectedPersonDetails();

        connectedOrganisation = new CreateConnectedOrganisation
        (
            category: ConnectedOrganisationCategory.RegisteredCompany,
            controlCondition: [ControlCondition.OwnsShares, ControlCondition.HasVotingRights],
            insolvencyDate: state.InsolvencyDate,
            lawRegistered: "Law_Registered",
            name: "org name",
            organisationId: null,
            registeredLegalForm: "Legal_Form"
        );


        List<Address> addresses = new();

        addresses.Add(new Address(
            countryName: "United Kingdom",
            locality: "Leeds",
            postalCode: "LS1 2AE",
            region: null,
            streetAddress: "1 street lane",
            streetAddress2: null,
            type: Organisation.WebApiClient.AddressType.Registered));

        addresses.Add(new Address(
            countryName: "United Kingdom",
            locality: "Leeds",
            postalCode: "LS1 2AE",
            region: null,
            streetAddress: "1 street lane",
            streetAddress2: null,
            type: Organisation.WebApiClient.AddressType.Postal));

        var registerConnectedEntity = new RegisterConnectedEntity
        (
            addresses: addresses,
            companyHouseNumber: "",
            endDate: null,
            entityType: state.ConnectedEntityType!.Value.AsApiClientConnectedEntityType(),
            hasCompnayHouseNumber: state.HasCompaniesHouseNumber!.Value,
            individualOrTrust: connectedIndividualTrust,
            organisation: connectedOrganisation,
            overseasCompanyNumber: "",
            registeredDate: state.RegistrationDate!.Value,
            registerName: state.RegisterName,
            startDate: null
        );

        return registerConnectedEntity;
    }

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Organisation,
            OrganisationName = "Org_name",
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678",
            ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares],
            RegistrationDate = new DateTimeOffset(2011, 7, 15, 0, 0, 0, TimeSpan.FromHours(0)),
            InsolvencyDate = new DateTimeOffset(2010, 6, 11, 0, 0, 0, TimeSpan.FromHours(0)),
            ConnectedEntityOrganisationCategoryType = ConnectedEntityOrganisationCategoryType.RegisteredCompany,
        };

        return connectedPersonDetails;
    }
}