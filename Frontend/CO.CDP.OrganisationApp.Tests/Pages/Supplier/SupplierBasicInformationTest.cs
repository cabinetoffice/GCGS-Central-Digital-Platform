using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier;

public class SupplierBasicInformationTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly SupplierBasicInformationModel _model;

    public SupplierBasicInformationTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierBasicInformationModel(_organisationClientMock.Object)
        {
            OperationTypes = new List<OperationType>()
        };
    }

    [Fact]
    public async Task OnGet_SetSupplierInformation()
    {
        var supplierInfo = SupplierDetailsFactory.CreateSupplierInformationClientModel();
        var organisationId = Guid.NewGuid();
        _organisationClientMock.Setup(o => o.GetOrganisationSupplierInformationAsync(organisationId))
            .ReturnsAsync(supplierInfo);

        _organisationClientMock.Setup(client => client.GetOrganisationAsync(organisationId))
            .ReturnsAsync(SupplierDetailsFactory.GivenOrganisationClientModel(organisationId));

        await _model.OnGet(organisationId);

        _model.VatNumber.Should().Be("FakeVatId");
        _model.SupplierInformation.Should().Be(supplierInfo);
        _model.SupplierInformation?.OperationTypes.Should().NotBeNull();
        _model.SupplierInformation?.OperationTypes.Should().BeOfType<List<OperationType>>();
        _model.SupplierInformation?.OperationTypes.Should().Contain(OperationType.SmallOrMediumSized);
        _model.SupplierInformation?.OperationTypes.Should().Contain(OperationType.NonGovernmental);
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