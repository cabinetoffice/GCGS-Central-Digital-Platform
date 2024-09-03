using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCompanyQuestionTest
{
    private readonly ConnectedEntityCompanyQuestionModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityCompanyQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityCompanyQuestionModel(_sessionMock.Object);
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
        _model.HasCompaniesHouseNumber = hasNumber;
        _model.CompaniesHouseNumber = "";
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
        state.CompaniesHouseNumber = "12345678";

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet(null);

        result.Should().BeOfType<PageResult>();
        _model.CompaniesHouseNumber.Should().Be("12345678");
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
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "law-register")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "law-register")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "law-enforces", true)]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "law-enforces", true)]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "legal-form-question")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "legal-form-question")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "postal-address-same-as-registered")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "postal-address-same-as-registered")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "Postal-address/uk", false, false)]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "Postal-address/uk", false, false)]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "Registered-address/uk")]
    public void OnGet_BackPageNameShouldBeExpectedPage(
        bool yesJourney,
            bool hasCompanyHouseNumber,
            ConnectedEntityType connectedEntityType,
            ConnectedEntityOrganisationCategoryType organisationCategoryType,
            string expectedBackPage,
            bool hasLegalForm = false,
            bool areSameAddress = true)
    {
        var state = DummyConnectedPersonDetails();
        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        state.HasLegalForm = hasLegalForm;
        _model.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        state.CompaniesHouseNumber = hasCompanyHouseNumber == true ? "12345678" : null;
        if (areSameAddress == false)
        {
            state.PostalAddress!.AddressLine1 = "New Street";
        }
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet(null);

        result.Should().BeOfType<PageResult>();
        _model.BackPageLink.Should().Be(expectedBackPage);
    }

    [Theory]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityControlCondition")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityControlCondition")]
    [InlineData(false, false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityOverseasCompanyQuestion")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCheckAnswersOrganisation")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCheckAnswersOrganisation")]
    [InlineData(false, false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityOverseasCompanyQuestion")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "ConnectedEntityCheckAnswersOrganisation")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "ConnectedEntityCheckAnswersOrganisation")]
    [InlineData(false, false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "ConnectedEntityOverseasCompanyQuestion")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityCompanyInsolvencyDate")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityCompanyInsolvencyDate")]
    [InlineData(false, false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityOverseasCompanyQuestion")]
    [InlineData(true, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityControlCondition")]
    [InlineData(false, true, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityControlCondition")]
    [InlineData(true, false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityOverseasCompanyQuestion")]
    [InlineData(false, false, ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityOverseasCompanyQuestion")]
    public void OnPost_ShouldRedirectToExpectedPage(
            bool yesJourney,
            bool hasCompanyHouseNumber,
            ConnectedEntityType connectedEntityType,
            ConnectedEntityOrganisationCategoryType organisationCategoryType,
            string expectedRedirectPage)
    {

        var state = DummyConnectedPersonDetails();
        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;

        _model.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);

    }

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Organisation,
            ConnectedEntityOrganisationCategoryType = Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany,
            OrganisationName = "Org_name",
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678",
            RegisteredAddress = GetDummyAddress(),
            PostalAddress = GetDummyAddress(),
        };

        return connectedPersonDetails;
    }
    private ConnectedEntityState.Address GetDummyAddress()
    {
        return new ConnectedEntityState.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", Country = "GB" };
    }
}