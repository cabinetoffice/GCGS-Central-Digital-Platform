using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.EntityVerificationClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationInternationalIdentificationTests
{
    private readonly Mock<ISession> _sessionMock = new();
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<IPponClient> _pponClientMock = new();
    private readonly Mock<IFlashMessageService> _flashMessageServiceMock = new();
    private static readonly Guid _organisationId = Guid.NewGuid();

    private OrganisationInternationalIdentificationModel CreateModel()
    {
        return new OrganisationInternationalIdentificationModel(
            _sessionMock.Object,
            _organisationClientMock.Object,
            _pponClientMock.Object,
            _flashMessageServiceMock.Object
        );
    }

    [Fact]
    public async Task OnGet_ShouldSetCountryAndOrganisationScheme()
    {
        var model = CreateModel();
        model.RegistrationDetails.OrganisationIdentificationCountry = "FR";
        model.RegistrationDetails.OrganisationScheme = "ABC";

        await model.OnGet();

        model.Country.Should().Be("FR");
        model.OrganisationScheme.Should().Be("ABC");
    }

    [Fact]
    public async Task OnGet_ShouldHandleIdentifierRegistriesApiExceptionGracefully()
    {
        var model = CreateModel();
        _pponClientMock
            .Setup(p => p.GetIdentifierRegistriesAsync(It.IsAny<string>()))
            .ThrowsAsync(new EntityVerificationClient.ApiException("", 404, "", null, null));

        Func<Task> act = async () => await model.OnGet();

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        var model = CreateModel();
        model.ModelState.AddModelError("OrganisationScheme", "Required");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToOrganisationDetailsSummary_WhenEntityVerificationNotFound()
    {
        var model = CreateModel();
        model.RedirectToSummary = true;
        model.RegistrationDetails.OrganisationScheme = "ABC";
        model.RegistrationDetails.OrganisationIdentificationNumber = "123";

        _organisationClientMock
            .Setup(o => o.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("", 404, "", null, null));

        _pponClientMock
            .Setup(p => p.GetIdentifiersAsync(It.IsAny<string>()))
            .ThrowsAsync(new EntityVerificationClient.ApiException("", 404, "", null, null));

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToOrganisationName_WhenEntityVerificationNotFound()
    {
        var model = CreateModel();
        model.RedirectToSummary = false;
        model.RegistrationDetails.OrganisationScheme = "ABC";
        model.RegistrationDetails.OrganisationIdentificationNumber = "123";

        _organisationClientMock
            .Setup(o => o.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("", 404, "", null, null));

        _pponClientMock
            .Setup(p => p.GetIdentifiersAsync(It.IsAny<string>()))
            .ThrowsAsync(new EntityVerificationClient.ApiException("", 404, "", null, null));

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>().Which.PageName.Should().Be("OrganisationName");
    }

    [Fact]
    public async Task OnPost_ShouldSetFlashMessage_WhenOrganisationAlreadyExists()
    {
        var model = CreateModel();
        model.Identifier = "ABC:123";
        model.OrganisationName = "Test Organisation";

        _organisationClientMock
            .Setup(o => o.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var result = await model.OnPost();

        _flashMessageServiceMock.Verify(api => api.SetFlashMessage(
            FlashMessageType.Important,
            StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyAlreadyRegistered_NotificationBanner,
            null,
            null,
            It.Is<Dictionary<string, string>>(d => d["organisationIdentifier"] == model.Identifier),
            It.Is<Dictionary<string, string>>(d => d["organisationName"] == model.OrganisationName)
        ),
        Times.Once);

        result.Should().BeOfType<PageResult>();
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(
                  additionalIdentifiers: null,
                  addresses: null,
                  contactPoint: null,
                  details: new Details(approval: null, pendingRoles: []),
                  id: _organisationId,
                  identifier: null,
                  name: "Test Org",
                  roles: new List<PartyRole>(),
                  type: OrganisationType.Organisation);
    }
}

