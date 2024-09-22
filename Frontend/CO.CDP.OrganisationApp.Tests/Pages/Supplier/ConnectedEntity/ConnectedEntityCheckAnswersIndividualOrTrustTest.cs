using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCheckAnswersIndividualOrTrustTest
{
    private readonly ConnectedEntityCheckAnswersIndividualOrTrustModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityCheckAnswersIndividualOrTrustTest()
    {
        _sessionMock = new Mock<ISession>();
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _model = new ConnectedEntityCheckAnswersIndividualOrTrustModel(_sessionMock.Object, _mockOrganisationClient.Object);
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
    [InlineData(true, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, "ConnectedPersonSummary", "company-register-name")]
    [InlineData(true, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual, "ConnectedPersonSummary", "date-registered")]
    [InlineData(true, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual, "ConnectedPersonSummary", "Registered-address/uk")]
    [InlineData(true, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, "ConnectedPersonSummary", "company-register-name")]
    [InlineData(true, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, "ConnectedPersonSummary", "date-registered")]
    [InlineData(true, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust, "ConnectedPersonSummary", "Registered-address/uk")]
    [InlineData(false, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, "ConnectedPersonSummary", "date-registered-question")]
    [InlineData(false, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, "ConnectedPersonSummary", "company-register-name", true)]
    [InlineData(false, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual, "ConnectedPersonSummary", "date-registered-question")]
    [InlineData(false, Constants.ConnectedEntityType.Individual, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual, "ConnectedPersonSummary", "Registered-address/uk")]
    [InlineData(false, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, "ConnectedPersonSummary", "date-registered-question")]
    [InlineData(false, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, "ConnectedPersonSummary", "company-register-name", true)]
    [InlineData(false, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, "ConnectedPersonSummary", "date-registered-question")]
    [InlineData(false, Constants.ConnectedEntityType.TrustOrTrustee, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust, "ConnectedPersonSummary", "Registered-address/uk")]


    public async Task OnPost_BackPageNameShouldBeExpectedPage(
            bool yesJourney,
            Constants.ConnectedEntityType connectedEntityType,
            Constants.ConnectedEntityIndividualAndTrustCategoryType trustCategoryType,
            string expectedRedirectPage, string expectedBackPageName,
            bool registrationDateHasValue = false)
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityIndividualAndTrustCategoryType = trustCategoryType;
        state.HasRegistartionDate = registrationDateHasValue;
        state.RegistrationDate = registrationDateHasValue == true ? DateTime.UtcNow : null;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = await _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);

        _model.BackPageLink.Should().Be(expectedBackPageName);
    }

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Individual,
            OrganisationName = "Org_name",
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678",
            ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares],
            RegistrationDate = new DateTimeOffset(2011, 7, 15, 0, 0, 0, TimeSpan.FromHours(0)),
            InsolvencyDate = new DateTimeOffset(2010, 6, 11, 0, 0, 0, TimeSpan.FromHours(0)),
            DirectorLocation = "United Kingdom",
            ConnectedEntityIndividualAndTrustCategoryType = ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual,
            RegisteredAddress = GetDummyAddress()
        };

        return connectedPersonDetails;
    }

    private OrganisationWebApiClient.ConnectedEntity DummyConnectedEntity()
    {
        var connectedIndividualOrTrust = new OrganisationWebApiClient.ConnectedIndividualTrust(
            category: ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual,
            connectedType: ConnectedPersonType.Individual,
            dateOfBirth: new DateTimeOffset(1973, 6, 11, 0, 0, 0, TimeSpan.FromHours(0)),
            controlCondition: [ControlCondition.OwnsShares, ControlCondition.HasVotingRights],
            firstName: "John",
            lastName: "Smith",
            nationality: "British",
            personId: null,
            residentCountry: "United Kingdom",
            id: 1
        );
        List<Address> addresses = new();

        addresses.Add(new Address(
            countryName: "United Kingdom",
            country: "GB",
            locality: "Leeds",
            postalCode: "LS1 2AE",
            region: null,
            streetAddress: "1 street lane",
            type: OrganisationWebApiClient.AddressType.Registered));

        var connectedEntity = new OrganisationWebApiClient.ConnectedEntity(
            addresses,
            "012345",
            new DateTimeOffset?(),
            OrganisationWebApiClient.ConnectedEntityType.Individual,
            true,
            new Guid(),
            connectedIndividualOrTrust,
            null,
            "org name",
            null,
            null,
            null
        );

        return connectedEntity;
    }
    private ConnectedEntityState.Address GetDummyAddress()
    {
        return new ConnectedEntityState.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", Country = "GB" };
    }
}