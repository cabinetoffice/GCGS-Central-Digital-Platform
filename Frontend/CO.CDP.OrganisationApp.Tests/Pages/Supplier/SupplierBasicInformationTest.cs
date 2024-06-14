using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierBasicInformationTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierBasicInformationModel _model;

    public SupplierBasicInformationTest()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierBasicInformationModel(_sessionMock.Object, _organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_SetSupplierInformation()
    {
        var organisationId = Guid.NewGuid();
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(SupplierDetailsFactory.CreateSupplierInformationClientModel());

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(organisationId))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(organisationId));

        await _model.OnGet(organisationId);

        _model.VatNumber.Should().Be("FakeVatId");
        _model.SupplierInformation.Should().Be(SupplierDetailsFactory.CreateSupplierInformationClientModel());
    }

    [Fact]
    public async Task OnGet_PageNotFound()
    {
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new ApiException("Unexpected error", 404, "", default, null));

        var result = await _model.OnGet(Guid.NewGuid());

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }
}