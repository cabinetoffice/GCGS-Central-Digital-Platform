using CO.CDP.EntityVerificationClient;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.ComponentModel.DataAnnotations;
using Identifier = CO.CDP.Organisation.WebApiClient.Identifier;
using WebApiClientOrganisation = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationInternationalIdentificationModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock = new();
    private readonly Mock<IPponClient> _pponClientMock = new();
    private static readonly Guid _organisationId = Guid.NewGuid();

    private OrganisationInternationalIdentificationModel CreateModel()
    {
        return new OrganisationInternationalIdentificationModel(
            _organisationClientMock.Object,
            _pponClientMock.Object
        );
    }

    [Fact]
    public async Task OnGet_RedirectsToUnavailablePage_WhenCountryIsNull()
    {
        var model = CreateModel();

        model.Country = null;

        var result = await model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationRegistrationUnavailable");
    }

    [Fact]
    public async Task OnGet_RedirectsToPageNotFound_WhenValidationFails()
    {
        var model = CreateModel();
        model.Country = "FR";
        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WebApiClientOrganisation.Organisation)null!);

        var result = await model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_PopulatesExistingIdentifiers_WhenValidationSucceeds()
    {
        var model = CreateModel();
        model.Id = _organisationId;
        model.Country = "FR";
        var organisation = GivenOrganisationClientModel();

        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(organisation);

        await model.OnGet();

        model.ExistingInternationalIdentifiers.Should().Contain("FR-COH");
    }

    [Fact]
    public async Task OnGet_Should_ReturnPageResult_When_OrganisationIsValid()
    {
        var model = CreateModel();
        model.Country = "FR"; // Valid country
        var validOrganisation = GivenOrganisationClientModel();

        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(validOrganisation);

        _pponClientMock
            .Setup(client => client.GetIdentifierRegistriesAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<IdentifierRegistries>());

        var result = await model.OnGet();

        result.Should().BeOfType<PageResult>();
        model.ExistingInternationalIdentifiers.Should().Contain("FR-COH");
        model.InternationalIdentifiers.Should().BeEmpty();
    }


    [Fact]
    public async Task OnPost_RedirectsToPageNotFound_WhenOrganisationNotFound()
    {
        var model = CreateModel();

        _organisationClientMock
            .Setup(client => client.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync((WebApiClientOrganisation.Organisation)null!);

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_UpdatesOrganisationIdentifiers_WhenModelStateIsValid()
    {
        var model = CreateModel();
        model.Id = _organisationId;

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        model.OrganisationScheme = "FR-DFG";
        model.RegistrationNumbers = new Dictionary<string, string?> { { "FR-DFG", "123456789" } };

        model.InternationalIdentifiers = new List<IdentifierRegistries>();

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnPost_ShouldTrimOrganisationIdentifiers_BeforeUpdate()
    {
        var model = CreateModel();
        model.Id = _organisationId;
        model.OrganisationScheme = "GB-COH";
        model.RegistrationNumbers = new Dictionary<string, string?> { { "GB-COH", "   1234ABCD " } };

        _organisationClientMock.Setup(x => x.GetOrganisationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        List<OrganisationIdentifier> capturedIdentifiers = [];

        _organisationClientMock
            .Setup(x => x.UpdateOrganisationAsync(model.Id, It.IsAny<UpdatedOrganisation>()))
            .Callback<Guid, UpdatedOrganisation>((_, updatedOrganisation) =>
            {
                capturedIdentifiers = updatedOrganisation.Organisation.AdditionalIdentifiers.ToList();
            })
            .Returns(Task.CompletedTask);


        var result = await model.OnPost();

        capturedIdentifiers.Should().HaveCount(1);
        capturedIdentifiers[0].Scheme.Should().Be("GB-COH");
        capturedIdentifiers[0].Id.Should().Be("1234ABCD");
    }

    [Fact]
    public void OrganisationScheme_ShouldBeRequired()
    {
        var model = CreateModel();
        model.Id = _organisationId;
        model.OrganisationScheme = null;

        var validationResult = ValidateModel(model);

        validationResult.Should().HaveCount(1);
        validationResult.Should().Contain(error => error.ErrorMessage == StaticTextResource.OrganisationRegistration_InternationalIdentifier_Type_Required_ErrorMessage);
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        var identifier = new Identifier("asd", "asd", "asd", new Uri("http://whatever"));
        var additionalIdentifiers = new List<Identifier>
                {
                    new Identifier(
                        id: "12345678",
                        legalName: "Mock Legal Name 1",
                        scheme: "FR-COH",
                        uri: new Uri("http://example.com/1")
                    )
                };

        return new CO.CDP.Organisation.WebApiClient.Organisation(
               additionalIdentifiers: additionalIdentifiers,
               addresses: null,
               contactPoint: null,
               details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null),
               id: _organisationId,
               identifier: identifier,
               name: "Test Org",
               roles: new List<PartyRole>(),
               type: OrganisationType.Organisation
               );
    }

    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationContext = new ValidationContext(model, serviceProvider: null, items: null);
        var validationResults = new List<ValidationResult>();

        Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}