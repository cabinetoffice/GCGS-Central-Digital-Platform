using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityLegalFormQuestionTest
{
    private readonly ConnectedEntityLegalFormQuestionModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityLegalFormQuestionTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityLegalFormQuestionModel(_sessionMock.Object);
        _model.Id = _organisationId;
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void OnPost_WhenOptionIsNullOrEmpty_ShouldReturnPageWithModelStateError(bool? hasLegalForm)
    {
        var state = DummyConnectedPersonDetails();
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);
        _model.HasLegalForm = hasLegalForm;
        _model.LegalFormName = "";
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
        state.LegalForm = "12345678";

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet(null);

        result.Should().BeOfType<PageResult>();
        _model.LegalFormName.Should().Be("12345678");
    }


    [Theory]
    [InlineData(true, "ConnectedEntityLawEnforce", Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany)]
    [InlineData(false, "ConnectedEntityCompanyQuestion", Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany)]
    public void OnPost_ShouldRedirectToExpectedPageWithSummary(bool hasLegalForm, string expectedRedirectPage, Constants.ConnectedEntityOrganisationCategoryType organisationCategoryType)
    {
        var state = DummyConnectedPersonDetails();
        _model.HasLegalForm = hasLegalForm;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
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
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "postal-address-same-as-registered")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "postal-address-same-as-registered")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "Postal-address/uk", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "Postal-address/uk", false)]
    public void OnPost_BackPageLinkNameShouldExpectedPage(
        ConnectedEntityType connectedEntityType,
        ConnectedEntityOrganisationCategoryType? organisationCategoryType,
        string expectedBackPageLinkName,
        bool areSameAddress = true)
    {
        var state = DummyConnectedPersonDetails();
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        if (areSameAddress == false)
        {
            state.PostalAddress!.AddressLine1 = "New Street";
        }
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);


        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        _model.BackPageLink.Should().Be(expectedBackPageLinkName);
    }

    [Theory]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityCompanyQuestion")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityLawEnforce", true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCompanyQuestion", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityLawEnforce", true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCheckAnswersOrganisation", null, true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCheckAnswersOrganisation", true, true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityCheckAnswersOrganisation", null, false, false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityCheckAnswersOrganisation", null, false, true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityLawEnforce", true, false, false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityCheckAnswersOrganisation", false, true, false)]
    public void OnPost_ShouldRedirectToExpectedPage(
        ConnectedEntityType connectedEntityType,
        ConnectedEntityOrganisationCategoryType? organisationCategoryType,
        string expectedRedirectPage,
        bool? hasLegalForm = false,
        bool yesJourney = false,
        bool hasCompanyHouseNumber = false)
    {
        var state = DummyConnectedPersonDetails();
        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        state.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        state.HasLegalForm = hasLegalForm;
        _model.HasLegalForm = hasLegalForm;
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
            SupplierHasCompanyHouseNumber = false,
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