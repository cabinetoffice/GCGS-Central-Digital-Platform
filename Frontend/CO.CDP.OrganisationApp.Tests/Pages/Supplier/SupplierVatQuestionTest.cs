using Amazon.S3;
using CO.CDP.EntityVerificationClient;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierVatModelQuestionTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly Mock<IPponClient> _pponClientMock;
    private static readonly Guid _organisationId = Guid.NewGuid();
    private readonly SupplierVatQuestionModel _model;

    public SupplierVatModelQuestionTest()
    {
        var httpContext = new DefaultHttpContext();
        var contextAccessor = new Mock<IHttpContextAccessor>();
        contextAccessor.Setup(_ => _.HttpContext).Returns(httpContext);

        _organisationClientMock = new Mock<IOrganisationClient>();
        _pponClientMock = new Mock<IPponClient>();
        _model = new SupplierVatQuestionModel(_organisationClientMock.Object, _pponClientMock.Object, contextAccessor.Object);
        
        _organisationClientMock.Setup(api => api.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new Organisation.WebApiClient.ApiException("Organisation does not exist", 404, "", null, null));
        _pponClientMock.Setup(api => api.GetIdentifiersAsync(It.IsAny<string>()))
            .ThrowsAsync(new EntityVerificationClient.ApiException("Organisation does not exist", 404, "", null, null));
    }

    [Fact]
    public async Task OnGet_ValidId_ReturnsPageResult()
    {
        var id = Guid.NewGuid();

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<PageResult>();
        _model.HasVatNumber.Should().Be(true);
        _model.VatNumber.Should().Be("FakeVatId");
    }

    [Fact]
    public async Task OnGet_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ThrowsAsync(new Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(id);

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ValidModelState_ReturnsRedirectToSupplierBasicInformation()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        _model.VatNumber = "VAT12345";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_WhenOrganisationExistsInOganisationService_ShouldRedirectToOrganisationAlreadyRegisteredPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        _model.VatNumber = "VAT12345";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        _organisationClientMock.Setup
            (api => api.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var result = await _model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("/Registration/OrganisationAlreadyRegistered");
    }

    [Fact]
    public async Task OnPost_WhenIdentifierExistsInEntityVerificationService_ShouldRedirectToOrganisationAlreadyRegisteredPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        _model.VatNumber = "VAT12345";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        _pponClientMock.Setup
            (api => api.GetIdentifiersAsync(It.IsAny<string>()))
            .ReturnsAsync(GivenEntityVerificationIdentifiers());

        var result = await _model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("/Registration/OrganisationAlreadyRegistered");
    }

    [Fact]
    public async Task OnPost_WhenEntityVerificationServiceOffLine_ShouldRedirectToOrganisationServiceUnavailablePage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        _model.VatNumber = "VAT12345";

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        _pponClientMock.Setup(api => api.GetIdentifiersAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Entity Verification service offline."));

        var result = await _model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("/Registration/OrganisationRegistrationUnavailable");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        _model.ModelState.AddModelError("HasVatNumber", "Please select an option");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_OrganisationNotFound_ReturnsRedirectToPageNotFound()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = false;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ThrowsAsync(new Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    private static Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, _organisationId, null, "Test Org", []);
    }

    private static ICollection<EntityVerificationClient.Identifier> GivenEntityVerificationIdentifiers()
    {
        return new List<EntityVerificationClient.Identifier>() {
            new EntityVerificationClient.Identifier("12345", "Acme Ltd", "VAT", new Uri("http://acme.org")) };
    }
}