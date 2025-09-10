using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.Net;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationNameSearchModelTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IFlashMessageService> _flashMessageServiceMock;
    private static readonly Guid _givenOrganisationId = Guid.NewGuid();

    public OrganisationNameSearchModelTests()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _flashMessageServiceMock = new Mock<IFlashMessageService>();
    }

    private OrganisationNameSearchModel GivenOrganisationNameSearchModel()
    {
        return new OrganisationNameSearchModel(
                    _sessionMock.Object,
                    _organisationClientMock.Object,
                    _flashMessageServiceMock.Object
                );
    }

    [Fact]
    public async Task OnGet_WhenOrganisationTypeIsNotBuyer_ShouldRedirectToOrganisationEmail()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test org", Constants.OrganisationType.Supplier);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameSearchModel();
        var result = await model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationEmail");
    }

    [Fact]
    public async Task OnGet_WhenOrganisationNameIsLessThanThreeCharacters_ShouldRedirectToOrganisationEmail()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("AB", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationNameSearchModel();
        var result = await model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationEmail");
    }

    [Fact]
    public async Task OnGet_WhenApiReturnsSingleExactMatch_ShouldRedirectToJoinOrganisationWithFlashMessage()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test org", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var matchingOrganisation = GivenOrganisationSearchResult("Test org", "scheme", "123", _givenOrganisationId);

        _organisationClientMock
            .Setup(client => client.SearchOrganisationAsync("Test org", "Buyer", 10, 0.3, false))
            .ReturnsAsync(new List<OrganisationSearchResult> { matchingOrganisation });

        var model = GivenOrganisationNameSearchModel();
        var result = await model.OnGet();

        _flashMessageServiceMock.Verify(service =>
            service.SetFlashMessage(
                FlashMessageType.Important,
                StaticTextResource.OrganisationRegistration_SearchOrganisationName_ExactMatchAlreadyExists,
                null,
                null,
                null,
                null
            ), Times.Once);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/registration/{_givenOrganisationId.ToString()}/join-organisation");
    }

    [Fact]
    public async Task OnGet_WhenLookupApiReturnsSingleExactMatch_ShouldRedirectToJoinOrganisationWithFlashMessage()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test org", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var matchingOrganisation = GivenOrganisationSearchResult("Test org", "scheme", "123");

        _organisationClientMock
            .Setup(client => client.LookupOrganisationAsync("Test org", String.Empty))
            .ReturnsAsync(GivenOrganisation());

        var model = GivenOrganisationNameSearchModel();
        var result = await model.OnGet();

        _flashMessageServiceMock.Verify(service =>
            service.SetFlashMessage(
                FlashMessageType.Important,
                StaticTextResource.OrganisationRegistration_SearchOrganisationName_ExactMatchAlreadyExists,
                null,
                null,
                null,
                null
            ), Times.Once);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/registration/{_givenOrganisationId.ToString()}/join-organisation");
    }

    [Fact]
    public async Task OnGet_WhenApiReturnsExactMatchSomewhereInResults_ShouldRedirectToJoinOrganisationWithFlashMessage()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test org", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var nonMatchingOrganisation1 = GivenOrganisationSearchResult("Something", "scheme", "124");
        var nonMatchingOrganisation2 = GivenOrganisationSearchResult("Another thing", "scheme", "125");
        var matchingOrganisation = GivenOrganisationSearchResult("Test org", "scheme", "123", _givenOrganisationId);

        _organisationClientMock
            .Setup(client => client.SearchOrganisationAsync("Test org", "Buyer", 10, 0.3, false))
            .ReturnsAsync(new List<OrganisationSearchResult> { nonMatchingOrganisation1, nonMatchingOrganisation2, matchingOrganisation });

        var model = GivenOrganisationNameSearchModel();
        var result = await model.OnGet();

        _flashMessageServiceMock.Verify(service =>
            service.SetFlashMessage(
                FlashMessageType.Important,
                StaticTextResource.OrganisationRegistration_SearchOrganisationName_ExactMatchAlreadyExists,
                null,
                null,
                null,
                null
            ), Times.Once);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/registration/{_givenOrganisationId.ToString()}/join-organisation");
    }

    [Fact]
    public async Task OnGet_WhenApiReturnsMultipleResults_ShouldReturnPage()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test org", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        _organisationClientMock
            .Setup(client => client.LookupOrganisationAsync("Test org", String.Empty))
            .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        var matchingOrganisations = new List<OrganisationSearchResult>
        {
            GivenOrganisationSearchResult("Test org 1"),
            GivenOrganisationSearchResult("Test org 2")
        };

        _organisationClientMock
            .Setup(client => client.SearchOrganisationAsync("Test org", "Buyer", 10, 0.3, false))
            .ReturnsAsync(matchingOrganisations);

        var model = GivenOrganisationNameSearchModel();

        var result = await model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.MatchingOrganisations.Should().HaveCount(2);
    }

    [Fact]
    public async Task OnPost_WhenApiReturnsMultipleResultsAndUserSelectsRequestToJoin_ShouldRedirectToJoinOrganisationPage()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test org", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        _organisationClientMock
            .Setup(client => client.LookupOrganisationAsync("Test org", String.Empty))
            .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        var matchingOrganisations = new List<OrganisationSearchResult>
        {
            GivenOrganisationSearchResult("Test org 1"),
            GivenOrganisationSearchResult("Test org 2")
        };

        _organisationClientMock
            .Setup(client => client.SearchOrganisationAsync("Test org", "Buyer", 10, 0.3, false))
            .ReturnsAsync(matchingOrganisations);

        var model = GivenOrganisationNameSearchModel();

        model.RequestToJoinOrganisationName = "Test org 1";
        model.OrganisationIdentifier = Guid.NewGuid().ToString();

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();

        var redirectResult = (RedirectToPageResult)result;
        redirectResult.PageName.Should().Be("JoinOrganisation");
        redirectResult.RouteValues.Should().ContainSingle();
        redirectResult.RouteValues?["id"].Should().Be(model.OrganisationIdentifier.ToString());
    }

    [Fact]
    public async Task OnPost_WhenRedirectToSummaryIsTrue_ShouldRedirectToOrganisationDetailsSummary()
    {
        var model = GivenOrganisationNameSearchModel();
        model.RedirectToSummary = true;

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsInvalid_ShouldReturnPage()
    {
        var model = GivenOrganisationNameSearchModel();
        model.ModelState.AddModelError("OrganisationIdentifier", "Required");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_WhenApiReturnsNoResults_ShouldRedirectToOrganisationEmail()
    {
        RegistrationDetails registrationDetails = GivenRegistrationDetails("Test Org", Constants.OrganisationType.Buyer);
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        _organisationClientMock
            .Setup(client => client.SearchOrganisationAsync("Test Org", "Buyer", 10, 0.3, false))
            .ThrowsAsync(new ApiException(string.Empty, (int)HttpStatusCode.NotFound, string.Empty, null, null));

        var model = GivenOrganisationNameSearchModel();
        var result = await model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationEmail");
    }

    [Fact]
    public async Task OnPost_WhenOrganisationIdentifierIsNone_ShouldRedirectToOrganisationEmail()
    {
        var model = GivenOrganisationNameSearchModel();
        model.OrganisationIdentifier = "None";

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationEmail");
    }


    private RegistrationDetails GivenRegistrationDetails(string organisationName, Constants.OrganisationType orgType)
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = organisationName,
            OrganisationEmailAddress = "test@example.com",
            OrganisationIdentificationNumber = "123456",
            OrganisationHasCompaniesHouseNumber = true,
            OrganisationType = orgType
        };

        return registrationDetails;
    }

    private static OrganisationSearchResult GivenOrganisationSearchResult(string name, string identifierScheme = "scheme", string idenfifierId = "123", Guid? organisationId = null)
    {
        return new OrganisationSearchResult(
                    organisationId ?? Guid.NewGuid(),
                    new Identifier(idenfifierId, "legal name", identifierScheme, new Uri("http://whatever")),
                    name,
                    new List<PartyRole>(),
                    new List<PartyRole>() { PartyRole.Buyer },
                    OrganisationType.Organisation
                );
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisation()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
                  additionalIdentifiers: null,
                  addresses: null,
                  contactPoint: null,
                  details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null),
                  id: _givenOrganisationId,
                  identifier: new Identifier("123", "Acme", "scheme", new Uri("http://www.acme.org")),
                  name: "Test Org",
                  roles: new List<PartyRole>(),
                  type: OrganisationType.Organisation
                  );
    }
}
