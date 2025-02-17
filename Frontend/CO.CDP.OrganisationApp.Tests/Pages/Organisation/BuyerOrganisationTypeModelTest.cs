using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class BuyerOrganisationTypeModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly BuyerOrganisationTypeModel _model;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;
    private readonly Guid _testOrganisationId = Guid.NewGuid();

    public BuyerOrganisationTypeModelTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new BuyerOrganisationTypeModel(_organisationClientMock.Object) 
            {
                Id = _testOrganisationId
            };
        
        _stringLocalizerMock = new Mock<IStringLocalizer>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationTypeModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }   
       
    [Fact]
    public async Task OnGet_ValidSession_ReturnsOrganisationDetailsAsync()
    {       
        _organisationClientMock.Setup(o => o.GetOrganisationBuyerInformationAsync(_testOrganisationId))
            .ReturnsAsync(GivenBuyerInformationClientModel(_testOrganisationId));

        await _model.OnGet();

        _organisationClientMock.Verify(c => c.GetOrganisationBuyerInformationAsync(_testOrganisationId), Times.Once);
    }

    [Fact]
    public async Task OnGet_WhenOrganisationExists_SetsBuyerOrganisationTypeAndReturnsPage()
    {        
        _organisationClientMock
            .Setup(client => client.GetOrganisationBuyerInformationAsync(_testOrganisationId))
            .ReturnsAsync(GivenBuyerInformationClientModel(_testOrganisationId));
       
        var result = await _model.OnGet();
       
        _model.BuyerOrganisationType.Should().Be("PublicUndertaking");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_WhenOrganisationDoesNotExist_RedirectsToPageNotFound()
    {
        _organisationClientMock
            .Setup(client => client.GetOrganisationBuyerInformationAsync(_testOrganisationId))
            .ThrowsAsync(new CDP.Organisation.WebApiClient.ApiException("Not Found", 404, null, null, null));

        var result = await _model.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_WhenModelStateIsInvalid_ReturnsPageResult()
    {      
        _model.ModelState.AddModelError("BuyerOrganisationType", "Required");

        var result = await _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WhenOrganisationDoesNotExist_RedirectsToPageNotFound()
    {
        _organisationClientMock
            .Setup(client => client.GetOrganisationBuyerInformationAsync(_testOrganisationId))
            .ThrowsAsync(new CDP.Organisation.WebApiClient.ApiException("Not Found", 404, null, null, null));

        var result = await _model.OnPost();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_WhenUpdateIsSuccessful_RedirectsToOrganisationOverview()
    {
        _organisationClientMock
            .Setup(client => client.GetOrganisationBuyerInformationAsync(_testOrganisationId))
            .ReturnsAsync(GivenBuyerInformationClientModel(_testOrganisationId));

        var result = await _model.OnPost();
        result.Should().NotBeNull();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }


    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: new ContactPoint("Main Contact", "contact@test.com", "123456789", null), id: id ?? Guid.NewGuid(), identifier: null, name: null, type: OrganisationType.Organisation, roles: [], details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
    }

    private static CO.CDP.Organisation.WebApiClient.BuyerInformation GivenBuyerInformationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.BuyerInformation(
            buyerType: "PublicUndertaking",
            devolvedRegulations: [DevolvedRegulation.NorthernIreland, DevolvedRegulation.Wales]);
    }

    private BuyerOrganisationTypeModel GivenOrganisationTypeModel()
    {
        return new BuyerOrganisationTypeModel(_organisationClientMock.Object);
    }
}