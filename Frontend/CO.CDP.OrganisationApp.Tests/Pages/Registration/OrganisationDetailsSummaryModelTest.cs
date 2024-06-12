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

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationDetailsSummaryModelTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private static readonly Guid _organisationId = Guid.NewGuid();

    public OrganisationDetailsSummaryModelTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        var model = GivenOrganisationDetailModel();

        model.OnGet();

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
            .Which.PageName.Should().Be("/OrganisationSelection");
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

        organisationClientMock.Verify(o => o.UpdateBuyerInformationAsync(_organisationId, It.IsAny<UpdateBuyerInformation>()), Times.AtLeastOnce);
        sessionMock.Verify(s => s.Remove(Session.RegistrationDetailsKey), Times.Once);

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/OrganisationSelection");
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
            .Which.PageName.Should().Be("/OrganisationSelection");
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
        Organisation.WebApiClient.Organisation? organisation = null;
        organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(organisation);

        var result = await model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Theory]
    [InlineData(ErrorCodes.ORGANISATION_ALREADY_EXISTS, ErrorMessagesList.DuplicateOgranisationName, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorCodes.ARGUMENT_NULL, ErrorMessagesList.PayLoadIssueOrNullAurgument, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorCodes.INVALID_OPERATION, ErrorMessagesList.OrganisationCreationFailed, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorCodes.PERSON_DOES_NOT_EXIST, ErrorMessagesList.PersonNotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.UNPROCESSABLE_ENTITY, ErrorMessagesList.UnprocessableEntity, StatusCodes.Status422UnprocessableEntity)]
    [InlineData(ErrorCodes.UNKNOWN_ORGANISATION, ErrorMessagesList.UnknownOrganisation, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.BUYER_INFO_NOT_EXISTS, ErrorMessagesList.BuyerInfoNotExists, StatusCodes.Status404NotFound)]
    [InlineData(ErrorCodes.UNKNOWN_BUYER_INFORMATION_UPDATE_TYPE, ErrorMessagesList.UnknownBuyerInformationUpdateType, StatusCodes.Status400BadRequest)]
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


    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com",
            OrganisationType = OrganisationType.Supplier,            
        };

        return registrationDetails;
    }

    private static Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new Organisation.WebApiClient.Organisation(null, null, null, _organisationId, null, "Test Org", []);
    }

    private OrganisationDetailsSummaryModel GivenOrganisationDetailModel()
    {
        var registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);

        return new OrganisationDetailsSummaryModel(sessionMock.Object, organisationClientMock.Object);
    }
}