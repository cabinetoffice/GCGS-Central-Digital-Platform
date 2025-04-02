using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierEmailAddressTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierEmailAddressModel _model;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;

    public SupplierEmailAddressTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierEmailAddressModel(_organisationClientMock.Object);
        _stringLocalizerMock = new Mock<IStringLocalizer>();
    }

    [Fact]
    public async Task OnGet_ValidId_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierInformationClientModel);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(OrganisationClientModel(id));

        _model.Id = id;
        var result = await _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.EmailAddress.Should().Be("test@test.com");
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        _model.Id = id;
        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.EmailAddress = "test@test.com";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(OrganisationClientModel(id));

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierInformationClientModel);

        _organisationClientMock.Setup(client => client.UpdateSupplierInformationAsync(id,
            It.IsAny<UpdateSupplierInformation>())).Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierInformationClientModel);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(OrganisationClientModel(id));

        _model.Id = id;
        _model.ModelState.AddModelError("EmailAddress", "Please enter an email");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
    {
        _model.EmailAddress = "dummy";

        _stringLocalizerMock
            .Setup(localizer => localizer[nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), StaticTextResource.Global_Email_Invalid_ErrorMessage));

        var validationContext = ValidationContextFactory.GivenValidationContextWithStringLocalizerFactory(_model, _stringLocalizerMock.Object);

        var results = ModelValidationHelper.Validate(_model, validationContext);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Global_Email_Invalid_ErrorMessage);
    }

    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _model.Id = id;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    private static SupplierInformation SupplierInformationClientModel => new(
            organisationName: "FakeOrg",
            supplierType: SupplierType.Organisation,
            operationTypes: null,
            completedRegAddress: true,
            completedPostalAddress: false,
            completedVat: false,
            completedWebsiteAddress: false,
            completedEmailAddress: true,
            completedOperationType: false,
            completedLegalForm: false,
            completedConnectedPerson: false,
            legalForm: null);

    private static CO.CDP.Organisation.WebApiClient.Organisation OrganisationClientModel(Guid id) =>
        new(
            additionalIdentifiers: [new Identifier(id: "FakeId", legalName: "FakeOrg", scheme: "VAT", uri: null)],
            addresses: null,
            contactPoint: new ContactPoint(email: "test@test.com", name: null, telephone: null, url: new Uri("https://xyz.com")),
            id: id,
            identifier: null,
            name: "Test Org",
            type: OrganisationType.Organisation,
            roles: [PartyRole.Supplier],
            details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)
        );
}