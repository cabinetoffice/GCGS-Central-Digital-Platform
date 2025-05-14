using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierVatModelQuestionTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private static readonly Guid _organisationId = Guid.NewGuid();
    private readonly SupplierVatQuestionModel _model;

    public SupplierVatModelQuestionTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierVatQuestionModel(_organisationClientMock.Object);

        _organisationClientMock.Setup(api => api.LookupOrganisationAsync(It.IsAny<string>(), It.IsAny<string>()))
                    .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("Organisation does not exist", 404, "", null, null));
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
            .ThrowsAsync(new CO.CDP.Organisation.WebApiClient.ApiException("Unexpected error", 404, "", default, null));

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

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");

        _organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(It.IsAny<Guid>(),
            It.Is<UpdateSupplierInformation>(u => u.Type == SupplierInformationUpdateType.CompletedVat)), Times.Once);
    }

    [Fact]
    public async Task OnPost_WhenCompanyDeregistersVatNumber_ShouldUpdateOrganisationAndRedirectToSupplierBasicInformationPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = false;
        var fakeOrg = SupplierDetailsFactory.GivenOrganisationClientModel(id);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(fakeOrg);

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        var result = await _model.OnPost();

        _organisationClientMock.Verify(o => o.UpdateOrganisationAsync(id, It.IsAny<UpdatedOrganisation>()), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("SupplierBasicInformation");

        _organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(It.IsAny<Guid>(),
            It.Is<UpdateSupplierInformation>(u => u.Type == SupplierInformationUpdateType.CompletedVat)), Times.Once);
    }

    [Fact]
    public async Task OnPost_WhenNoChangeToVatNumber_ShouldNotUpdateOrganisationAndRedirectToSupplierBasicInformationPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        var fakeOrg = SupplierDetailsFactory.GivenOrganisationClientModel(id);
        _model.VatNumber = fakeOrg.AdditionalIdentifiers.First().Id;

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(fakeOrg);

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        await _model.OnPost();

        _organisationClientMock.Verify(o => o.UpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<UpdatedOrganisation>()), Times.Never);
        _organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(It.IsAny<Guid>(),
            It.Is<UpdateSupplierInformation>(u => u.Type == SupplierInformationUpdateType.CompletedVat)), Times.Never);
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.ModelState.AddModelError("HasVatNumber", "Please select an option");

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

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
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>().Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldTrimOrganisationIdentifiers_BeforeUpdate()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.HasVatNumber = true;
        _model.VatNumber = " \t 1234ABCD \n";

        _organisationClientMock.Setup(client => client.GetOrganisationSupplierInformationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(id))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(id));

        List<OrganisationIdentifier> capturedIdentifiers = [];

        _organisationClientMock
            .Setup(x => x.UpdateOrganisationAsync(id, It.IsAny<UpdatedOrganisation>()))
            .Callback<Guid, UpdatedOrganisation>((_, updatedOrganisation) =>
            {
                capturedIdentifiers = updatedOrganisation.Organisation.AdditionalIdentifiers.ToList();
            })
            .Returns(Task.CompletedTask);


        var result = await _model.OnPost();

        capturedIdentifiers.Should().HaveCount(1);
        capturedIdentifiers[0].Id.Should().Be("1234ABCD");
    }
}