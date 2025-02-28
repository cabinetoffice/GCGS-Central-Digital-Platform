using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;
public class SupplierToBuyerOrganisationDetailsSummaryTest
{
    private readonly Mock<ITempDataService> tempDataServiceMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly SupplierToBuyerOrganisationDetailsSummaryModel _model;
    private readonly Guid orgId = Guid.NewGuid();

    public SupplierToBuyerOrganisationDetailsSummaryTest()
    {
        tempDataServiceMock = new Mock<ITempDataService>();
        organisationClientMock = new Mock<IOrganisationClient>();
        _model = new SupplierToBuyerOrganisationDetailsSummaryModel(
            tempDataServiceMock.Object,
            organisationClientMock.Object
        )
        {
            Id = orgId
        };
    }

    [Fact]
    public void OnGet_ShouldRedirectToOrganisationType_WhenBuyerOrganisationTypeIsNull()
    {
        tempDataServiceMock
            .Setup(td => td.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{orgId}_Answers"))
            .Returns(new SupplierToBuyerDetails { BuyerOrganisationType = null });

        var result = _model.OnGet();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationType");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgId);
    }

    [Fact]
    public void OnGet_ShouldReturnPage_WhenStateIsValid()
    {
        var supplierToBuyerDetails = new SupplierToBuyerDetails
        {
            OrganisationType = Constants.OrganisationType.Supplier,
            BuyerOrganisationType = "CentralGovernment",
            Devolved = true,
            Regulations = [Constants.DevolvedRegulation.Scotland]
        };

        tempDataServiceMock
            .Setup(td => td.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{orgId}_Answers"))
            .Returns(supplierToBuyerDetails);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.SupplierToBuyerDetailsModel.Should().NotBeNull();
        _model.SupplierToBuyerDetailsModel?.BuyerOrganisationType.Should().Be("CentralGovernment");
        _model.SupplierToBuyerDetailsModel?.Devolved.Should().BeTrue();
    }

    [Fact]
    public async Task OnPost_ShouldUpdateOrganisationAndRedirectToOverview_WhenBuyerOrganisationTypeIsNotNull()
    {
        var supplierToBuyerDetails = new SupplierToBuyerDetails
        {
            BuyerOrganisationType = "CentralGovernment",
            Devolved = true,
            Regulations = [Constants.DevolvedRegulation.Scotland]
        };

        tempDataServiceMock
            .Setup(td => td.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{orgId}_Answers"))
            .Returns(supplierToBuyerDetails);

        organisationClientMock
            .Setup(client => client.UpdateOrganisationAsync(
                orgId,
                It.Is<UpdatedOrganisation>(uo =>
                    uo.Type == OrganisationUpdateType.AddAsBuyerRole &&
                    uo.Organisation.BuyerInformation!.BuyerType == "CentralGovernment"
                )))
            .Returns(Task.CompletedTask);

        var result = await _model.OnPost();

        organisationClientMock.Verify(
            client => client.UpdateOrganisationAsync(
                orgId,
                It.Is<UpdatedOrganisation>(uo =>
                    uo.Type == OrganisationUpdateType.AddAsBuyerRole &&
                    uo.Organisation.BuyerInformation!.BuyerType == "CentralGovernment"
                )),
            Times.Once);

        tempDataServiceMock.Verify(td => td.Remove($"Supplier_To_Buyer_{orgId}_Answers"), Times.Once);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationOverview");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgId);
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToOrganisationType_WhenBuyerOrganisationTypeIsNull()
    {
        var supplierToBuyerDetails = new SupplierToBuyerDetails
        {
            BuyerOrganisationType = null
        };

        tempDataServiceMock
            .Setup(td => td.PeekOrDefault<SupplierToBuyerDetails>($"Supplier_To_Buyer_{orgId}_Answers"))
            .Returns(supplierToBuyerDetails);

        var result = await _model.OnPost();

        organisationClientMock.Verify(
            client => client.UpdateOrganisationAsync(It.IsAny<Guid>(), It.IsAny<UpdatedOrganisation>()),
            Times.Never);

        tempDataServiceMock.Verify(td => td.Remove(It.IsAny<string>()), Times.Never);
        tempDataServiceMock.Verify(td => td.Put(It.IsAny<string>(), It.IsAny<FlashMessage>()), Times.Never);

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("SupplierToBuyerOrganisationType");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(orgId);
    }
}
