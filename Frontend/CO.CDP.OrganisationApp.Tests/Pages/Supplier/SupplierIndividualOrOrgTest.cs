using CO.CDP.Organisation.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierIndividualOrOrgTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public SupplierIndividualOrOrgTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public async Task OnGet_SetSupplierInformationAndReturnPage()
    {
        var model = SupplierDetailsFactory.GivenSupplierIndividualOrOrgModel(sessionMock, organisationClientMock);
        var supplierInformation = SupplierDetailsFactory.CreateSupplierInformationClientModel();

        organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ReturnsAsync(supplierInformation);

        var result = await model.OnGet(Guid.NewGuid());

        model.SupplierType.Should().Be(SupplierType.Organisation);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        var model = SupplierDetailsFactory.GivenSupplierIndividualOrOrgModel(sessionMock, organisationClientMock);

        organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPage()
    {
        var model = SupplierDetailsFactory.GivenSupplierIndividualOrOrgModel(sessionMock, organisationClientMock);

        model.ModelState.AddModelError("error", "some error");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ValidModelState_UpdatesSupplierInformationAndRedirects()
    {
        var model = SupplierDetailsFactory.GivenSupplierIndividualOrOrgModel(sessionMock, organisationClientMock);
        var id = Guid.NewGuid();
        model.Id = id;
        model.SupplierType = SupplierType.Organisation;

        var result = await model.OnPost();

        organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(id,
            It.Is<UpdateSupplierInformation>(u => u.SupplierInformation.SupplierType == SupplierType.Organisation)), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("SupplierBasicInformation");

        (result as RedirectToPageResult)?.RouteValues?.GetValueOrDefault("Id").Should().Be(id);
    }

    [Fact]
    public async Task OnPost_ValidModelState_ThrowsApiException_ShouldRedirectToPageNotFound()
    {
        var model = SupplierDetailsFactory.GivenSupplierIndividualOrOrgModel(sessionMock, organisationClientMock);
        var id = Guid.NewGuid();
        model.Id = id;
        model.SupplierType = SupplierType.Organisation;

        organisationClientMock.Setup(o => o.UpdateSupplierInformationAsync(id,
            It.IsAny<UpdateSupplierInformation>())).ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

}