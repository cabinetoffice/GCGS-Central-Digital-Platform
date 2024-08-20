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
    public async Task OnGetChange_ShouldGetConnectedEntityAndLoadIntoSession_WhenIdAndConnectedIdSet()
    {
        _mockOrganisationClient
            .Setup(c => c.GetConnectedEntityAsync(_organisationId, _entityId))
            .ReturnsAsync(DummyConnectedEntity());

        _model.ConnectedEntityId = _entityId;

        await _model.OnGetChange(_entityId);

        _sessionMock.Verify(s => s.Set(Session.ConnectedPersonKey, It.Is<ConnectedEntityState>(v =>
            v.CompaniesHouseNumber == "012345")), Times.Once);
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

    [Fact]
    public async Task OnPost_ShouldUpdateConnectedPerson_WhenConnectedEntityIdIsSet()
    {
        var state = DummyConnectedPersonDetails();

        _model.ConnectedEntityId = _entityId;

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        await _model.OnPost();

        _mockOrganisationClient.Verify(c => c.UpdateConnectedEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<UpdateConnectedEntity>()), Times.Once());
    }

    [Theory]
    [InlineData(true, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedPersonSummary", "company-register-name")]
    [InlineData(true, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedPersonSummary", "company-question")]
    [InlineData(true, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "ConnectedPersonSummary", "company-question")]
    [InlineData(true, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedPersonSummary", "date-insolvency")]
    [InlineData(true, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedPersonSummary", "law-register")]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedPersonSummary", "company-register-name", false, true)]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedPersonSummary", "company-register-name", true, true)]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedPersonSummary", "date-registered-question", false, false)]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedPersonSummary", "overseas-company-question", false)]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "ConnectedPersonSummary", "overseas-company-question", false)]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedPersonSummary", "date-insolvency", false)]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedPersonSummary", "legal-form-question", false)]

    public async Task OnPost_BackPageNameShouldBeExpectedPage(
            bool yesJourney,
            Constants.ConnectedEntityType connectedEntityType,
            Constants.ConnectedEntityOrganisationCategoryType organisationCategoryType,
            string expectedRedirectPage, string expectedBackPageName,
            bool hasCompanyHouseNumber = true, bool registrationDateHasValue = false)
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        state.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        state.RegistrationDate = registrationDateHasValue == true ? DateTime.UtcNow : null;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);

        _model.BackPageLink.Should().Be(expectedBackPageName);
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
            type: Organisation.WebApiClient.AddressType.Registered));

        addresses.Add(new Address(
            countryName: "United Kingdom",
            locality: "Leeds",
            postalCode: "LS1 2AE",
            region: null,
            streetAddress: "1 street lane",
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
            RegistrationDate = new DateTime(2011, 7, 15, 0, 0, 0),
            InsolvencyDate = new DateTime(2010, 6, 11, 0, 0, 0),
            ConnectedEntityOrganisationCategoryType = ConnectedEntityOrganisationCategoryType.RegisteredCompany,
        };

        return connectedPersonDetails;
    }

    private Organisation.WebApiClient.ConnectedEntity DummyConnectedEntity()
    {
        var connectedOrganisation = new Organisation.WebApiClient.ConnectedOrganisation(
            ConnectedOrganisationCategory.RegisteredCompany,
            [ControlCondition.OwnsShares, ControlCondition.HasVotingRights],
            1,
            new DateTime(),
            "Law registered text",
            "Name text",
            new Guid(),
            "registeredLegalForm text"
        );

        var connectedEntity = new Organisation.WebApiClient.ConnectedEntity(
            new List<Address>(),
            "012345",
            new DateTime?(),
            Organisation.WebApiClient.ConnectedEntityType.Organisation,
            true,
            new Guid(),
            null,
            connectedOrganisation,
            "org name",
            null,
            null,
            null
        );

        return connectedEntity;
    }
}