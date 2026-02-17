using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using static CO.CDP.OrganisationApp.Tests.Pages.Registration.OrganisationEntityFactory;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationDetailsSummaryModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly Mock<EntityVerificationClient.IPponClient> pponClient;
    private readonly Mock<IFlashMessageService> _flashMessageServiceMock;
    private static readonly Guid _organisationId = Guid.NewGuid();

    public OrganisationDetailsSummaryModelTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
        organisationClientMock = new Mock<IOrganisationClient>();
        pponClient = new Mock<EntityVerificationClient.IPponClient>();
        _flashMessageServiceMock = new Mock<IFlashMessageService>();
    }

    [Fact]
    public async Task OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenOrganisationDetailModel();

        await model.OnGet();

        model.RegistrationDetails.As<RegistrationDetails>().Should().NotBeNull();
    }

    [Fact]
    public async Task OnPost_ValidSession_ShouldRegisterOrganisation()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_OnSuccess_RedirectsToOrganisationSelection()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_ValidOrganisation_UpdatesBuyerInformationAndRedirects()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        model.RegistrationDetails.BuyerOrganisationType = "BuyerOrgType";
        model.RegistrationDetails.Devolved = false;

        var buyerInfo = new UpdateBuyerInformation(
                    type: BuyerInformationUpdateType.BuyerOrganisationType,
                    buyerInformation: new BuyerInformation(
                        buyerType: model.RegistrationDetails.BuyerOrganisationType,
                        devolvedRegulations: []));



        var actionResult = await model.OnPost();

        organisationClientMock.Verify(
            o => o.UpdateBuyerInformationAsync(
                _organisationId,
                It.Is<UpdateBuyerInformation>(b => b.Type == buyerInfo.Type
                && b.BuyerInformation.BuyerType == buyerInfo.BuyerInformation.BuyerType)),
            Times.AtLeastOnce);
        sessionMock.Verify(s => s.Remove(Session.RegistrationDetailsKey), Times.Once);

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_DevolvedTrue_UpdatesDevolvedRegulationAndRedirects()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        model.RegistrationDetails.BuyerOrganisationType = null;
        model.RegistrationDetails.Devolved = true;
        model.RegistrationDetails.Regulations = [Constants.DevolvedRegulation.NorthernIreland, Constants.DevolvedRegulation.Wales];

        var result = await model.OnPost();

        organisationClientMock.Verify(o => o.UpdateBuyerInformationAsync(_organisationId, It.Is<UpdateBuyerInformation>(u => u.Type == BuyerInformationUpdateType.DevolvedRegulation)), Times.Once);

        sessionMock.Verify(s => s.Remove(Session.RegistrationDetailsKey), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_DevolvedFalse_UpdatesDevolvedRegulationAndRedirects()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        model.RegistrationDetails.BuyerOrganisationType = null;
        model.RegistrationDetails.Devolved = false;
        model.RegistrationDetails.Regulations = [DevolvedRegulation.NorthernIreland, DevolvedRegulation.Wales];

        var result = await model.OnPost();

        organisationClientMock.Verify(o => o.UpdateBuyerInformationAsync(_organisationId, It.IsAny<UpdateBuyerInformation>()), Times.Never);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPage()
    {
        var model = GivenOrganisationDetailModel();
        model.ModelState.AddModelError("error", "some error");

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_NullOrganisation_ReturnsPage()
    {
        var model = GivenOrganisationDetailModel();
        CO.CDP.Organisation.WebApiClient.Organisation? organisation = null;
        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(organisation);

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    public static IEnumerable<object[]> Get_OnPost_AddsModelError_TestData()
    {        
        yield return new object[] { ErrorCodes.ARGUMENT_NULL, ErrorMessagesList.PayLoadIssueOrNullArgument, StatusCodes.Status400BadRequest };
        yield return new object[] { ErrorCodes.INVALID_OPERATION, ErrorMessagesList.OrganisationCreationFailed, StatusCodes.Status400BadRequest };
        yield return new object[] { ErrorCodes.PERSON_DOES_NOT_EXIST, ErrorMessagesList.PersonNotFound, StatusCodes.Status404NotFound };
        yield return new object[] { ErrorCodes.UNPROCESSABLE_ENTITY, ErrorMessagesList.UnprocessableEntity, StatusCodes.Status422UnprocessableEntity };
        yield return new object[] { ErrorCodes.UNKNOWN_ORGANISATION, ErrorMessagesList.UnknownOrganisation, StatusCodes.Status404NotFound };
        yield return new object[] { ErrorCodes.BUYER_INFO_NOT_EXISTS, ErrorMessagesList.BuyerInfoNotExists, StatusCodes.Status404NotFound };
        yield return new object[] { ErrorCodes.UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE, ErrorMessagesList.UnknownBuyerInformationUpdateType, StatusCodes.Status400BadRequest };
        yield return new object[] { ErrorCodes.MOU_DOES_NOT_EXIST, ErrorMessagesList.MouNotFound, StatusCodes.Status404NotFound };
    }

    [Theory]
    [MemberData(nameof(Get_OnPost_AddsModelError_TestData))]
    public async Task OnPost_AddsModelError(string errorCode, string expectedErrorMessage, int statusCode)
    {
        var problemDetails = GivenProblemDetails(code: errorCode, statusCode: statusCode);
        var aex = GivenApiException(statusCode: statusCode, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);

        var model = GivenOrganisationDetailModel();

        await model.OnPost();

        model.ModelState[string.Empty].As<ModelStateEntry>().Errors
            .Should().Contain(e => e.ErrorMessage == expectedErrorMessage);
    }

    [Fact]
    public async Task OnPost_WhenOrganisationAlreadyExists_ShouldShowNotificationBanner()
    {        
        var model = GivenOrganisationDetailModel();

        var problemDetails = GivenProblemDetails(code: ErrorCodes.ORGANISATION_ALREADY_EXISTS, statusCode: StatusCodes.Status400BadRequest);
        var aex = GivenApiException(statusCode: StatusCodes.Status400BadRequest, problemDetails: problemDetails);

        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ThrowsAsync(aex);
        
        var matchingOrganisation = OrganisationSearchResult("TestOrg", "GB-COH", "123");

        organisationClientMock
            .Setup(client => client.LookupOrganisationAsync("TestOrg", ""))
            .ReturnsAsync(matchingOrganisation);

        var result = await model.OnPost();

        Dictionary<string, string> urlParameters = new() { ["organisationIdentifier"] = matchingOrganisation.Id.ToString() };
        Dictionary<string, string> htmlParameters = new() { ["organisationName"] = matchingOrganisation.Name };

        var heading = StaticTextResource.OrganisationRegistration_CompanyHouseNumberQuestion_CompanyAlreadyRegistered_NotificationBanner;

        _flashMessageServiceMock.Verify(api => api.SetFlashMessage(
            FlashMessageType.Important,
            heading,
            null,
            null,
            It.Is<Dictionary<string, string>>(d => d["organisationIdentifier"] == matchingOrganisation.Id.ToString()),
            It.Is<Dictionary<string, string>>(d => d["organisationName"] == matchingOrganisation.Name)
        ),
        Times.Once);


        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_SupplierWithOperationTypes_UpdatesSupplierInformationAndRedirects()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        model.RegistrationDetails.OrganisationType = Constants.OrganisationType.Supplier;
        model.RegistrationDetails.SupplierOrganisationOperationTypes = [OperationType.SmallOrMediumSized, OperationType.NonGovernmental];

        var result = await model.OnPost();

        organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(
            _organisationId,
            It.Is<UpdateSupplierInformation>(u =>
                u.Type == SupplierInformationUpdateType.OperationType &&
                u.SupplierInformation.OperationTypes != null &&
                u.SupplierInformation.OperationTypes.Count == 2)),
            Times.Once);

        sessionMock.Verify(s => s.Remove(Session.RegistrationDetailsKey), Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_SupplierWithEmptyOperationTypes_DoesNotUpdateSupplierInformation()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        model.RegistrationDetails.OrganisationType = Constants.OrganisationType.Supplier;
        model.RegistrationDetails.SupplierOrganisationOperationTypes = [];

        var result = await model.OnPost();

        organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(
            It.IsAny<Guid>(),
            It.Is<UpdateSupplierInformation>(u => u.Type == SupplierInformationUpdateType.OperationType)),
            Times.Never);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnPost_SupplierWithNoneOperationType_UpdatesSupplierInformationWithNone()
    {
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenOrganisationDetailModel();

        model.RegistrationDetails.OrganisationType = Constants.OrganisationType.Supplier;
        model.RegistrationDetails.SupplierOrganisationOperationTypes = [OperationType.None];

        var result = await model.OnPost();

        organisationClientMock.Verify(o => o.UpdateSupplierInformationAsync(
            _organisationId,
            It.Is<UpdateSupplierInformation>(u =>
                u.Type == SupplierInformationUpdateType.OperationType &&
                u.SupplierInformation.OperationTypes != null &&
                u.SupplierInformation.OperationTypes.Count == 1 &&
                u.SupplierInformation.OperationTypes.Contains(OperationType.None))),
            Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com",
            OrganisationType = Constants.OrganisationType.Supplier,
        };

        return registrationDetails;
    }   

    private static CO.CDP.Organisation.WebApiClient.Organisation OrganisationSearchResult(string name, string identifierScheme = "scheme", string idenfifierId = "123")
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null,
            id: Guid.NewGuid(), identifier: new Identifier(id:idenfifierId, legalName: "legal name", scheme: identifierScheme, uri: new Uri("http://whatever")),
            name: name,
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation, roles: [PartyRole.Buyer],
            details: new Details(approval: null, buyerInformation: null,
                                pendingRoles: [], publicServiceMissionOrganization: null, scale: null,
                                shelteredWorkshop: null, vcse: null));
    }


    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null,
            id: _organisationId, identifier: null,
            name: "Test Org",
            type: CDP.Organisation.WebApiClient.OrganisationType.Organisation, roles: [],
            details: new Details(approval: null, buyerInformation: null,
                                pendingRoles: [], publicServiceMissionOrganization: null, scale: null,
                                shelteredWorkshop: null, vcse: null));
    }

    private OrganisationDetailsSummaryModel GivenOrganisationDetailModel()
    {
        var registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);

        return new OrganisationDetailsSummaryModel(sessionMock.Object, organisationClientMock.Object, pponClient.Object, _flashMessageServiceMock.Object);
    }
}