using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityOverseasCompanyQuestionTest
{
    private readonly ConnectedEntityOverseasCompanyQuestionModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityOverseasCompanyQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityOverseasCompanyQuestionModel(_sessionMock.Object);
        _model.Id = _organisationId;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void OnPost_WhenOptionIsNullOrEmpty_ShouldReturnPageWithModelStateError(bool? hasNumber)
    {
        var state = DummyConnectedPersonDetails();
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);
        _model.HasOverseasCompaniesHouseNumber = hasNumber;
        _model.OverseasCompaniesHouseNumber = "";
        _model.ModelState.AddModelError("Question", "Please select an option");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _sessionMock
           .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
           .Returns((ConnectedEntityState?)null);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntitySupplierHasControl_WhenModelStateIsInvalid()
    {
        ConnectedEntityState? state = null;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnGet(null);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierHasControl");
    }

    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        var state = DummyConnectedPersonDetails();
        state.OverseasCompaniesHouseNumber = "12345678";

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet(null);

        result.Should().BeOfType<PageResult>();
        _model.OverseasCompaniesHouseNumber.Should().Be("12345678");
    }


    [Theory]
    [InlineData("ConnectedEntityControlCondition")]
    public void OnPost_ShouldRedirectToExpectedPage(string expectedRedirectPage)
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(s => s.Set(Session.ConnectedPersonKey, It.Is<ConnectedEntityState>(st => st.ConnectedEntityType == Constants.ConnectedEntityType.Organisation)), Times.Once);

    }
    [Theory]
    [InlineData(false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityControlCondition", "company-question")]
    [InlineData(false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCheckAnswersOrganisation", "company-question")]
    [InlineData(false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "ConnectedEntityCheckAnswersOrganisation", "company-question")]
    [InlineData(false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityCompanyInsolvencyDate", "company-question")]
    [InlineData(false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityControlCondition", "company-question")]
    public void OnPost_BackPageNameShouldBeExpectedPage(
            bool yesJourney,
            ConnectedEntityType connectedEntityType,
            ConnectedEntityOrganisationCategoryType organisationCategoryType,
            string expectedRedirectPage,
            string expectedBackPageName)
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);
        _model.BackPageLink.Should().Be(expectedBackPageName);
    }
    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Organisation,
            OrganisationName = "Org_name",
            HasCompaniesHouseNumber = false,
            HasOverseasCompaniesHouseNumber = true,
            OverseasCompaniesHouseNumber = "12345678",
            ConnectedEntityOrganisationCategoryType= ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl
        };

        return connectedPersonDetails;
    }
}