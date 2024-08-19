using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityControlConditionTest
{
    private readonly ConnectedEntityControlConditionModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityControlConditionTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityControlConditionModel(_sessionMock.Object) { ControlConditions = [] };
        _model.Id = _organisationId;
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

    public static IEnumerable<object[]> Guids
    {
        get
        {
            var v = (Guid?)null;
            yield return new object[] { v!, "ConnectedEntitySupplierCompanyQuestion" };
            yield return new object[] { Guid.NewGuid(), "ConnectedEntityCheckAnswersOrganisation" };
        }
    }

    [Theory, MemberData(nameof(Guids))]
    public void OnGet_ShouldRedirectToExpectedRedirectPage_WhenModelStateIsInvalid
        (Guid? connectedEntityId, string expectedRedirectPage)
    {
        ConnectedEntityState? state = null;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);
        _model.ConnectedEntityId = connectedEntityId;

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be(expectedRedirectPage);
    }

    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.ControlConditions.Should().Contain(Constants.ConnectedEntityControlCondition.OwnsShares);
    }

    [Theory]    
    [InlineData(true, ConnectedEntityType.Organisation, "overseas-company-question", ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, null, null)]
    [InlineData(true, ConnectedEntityType.Organisation, "company-question", ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, null, true)]
    [InlineData(true, ConnectedEntityType.Organisation, "company-question", ConnectedEntityOrganisationCategoryType.RegisteredCompany, null, null)]
    [InlineData(false, ConnectedEntityType.Organisation, "company-question", ConnectedEntityOrganisationCategoryType.RegisteredCompany, null, true)]
    [InlineData(false, ConnectedEntityType.Organisation, "overseas-company-question", ConnectedEntityOrganisationCategoryType.RegisteredCompany, null, false)]
    [InlineData(false, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, false)]
    [InlineData(false, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, true)]
    [InlineData(true, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, false)]
    [InlineData(true, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, true)]
    [InlineData(false, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual, false)]
    [InlineData(false, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual, true)]
    [InlineData(true, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual, false)]
    [InlineData(true, ConnectedEntityType.Individual, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual, true)]
    [InlineData(false, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, false)]
    [InlineData(false, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, true)]
    [InlineData(true, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, false)]
    [InlineData(true, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, true)]
    [InlineData(false, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, false)]
    [InlineData(false, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, true)]
    [InlineData(true, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, false)]
    [InlineData(true, ConnectedEntityType.TrustOrTrustee, "Registered-address/uk", null, ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust, true)]
    [InlineData(false, ConnectedEntityType.Organisation, "", ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, null, null)]
    [InlineData(true, ConnectedEntityType.Organisation, "", ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, null, null)]
    [InlineData(false, ConnectedEntityType.Organisation, "", ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, null, null)]
    [InlineData(true, ConnectedEntityType.Organisation, "", ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, null, null)]
    [InlineData(false, ConnectedEntityType.Organisation, "", ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, null, null)]
    [InlineData(true, ConnectedEntityType.Organisation, "", ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, null, null)]
    [InlineData(false, ConnectedEntityType.Individual, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual, false)]
    [InlineData(false, ConnectedEntityType.Individual, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual, true)]
    [InlineData(true, ConnectedEntityType.Individual, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual, false)]
    [InlineData(true, ConnectedEntityType.Individual, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual, true)]
    [InlineData(false, ConnectedEntityType.TrustOrTrustee, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust, false)]
    [InlineData(false, ConnectedEntityType.TrustOrTrustee, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust, true)]
    [InlineData(true, ConnectedEntityType.TrustOrTrustee, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust, false)]
    [InlineData(true, ConnectedEntityType.TrustOrTrustee, "", null, ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust, true)]

    public void OnGet_ShouldReturnBackPageResult(
            bool yesJourney,
            ConnectedEntityType connectedEntityType,
            string expectedBackPage,
            ConnectedEntityOrganisationCategoryType? categoryType,
            ConnectedEntityIndividualAndTrustCategoryType? individualAndTrustCategoryType,
            bool? hasCompaniesHouseNumber)
    {
        var state = DummyConnectedPersonDetails();
        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = categoryType;
        state.ConnectedEntityIndividualAndTrustCategoryType = individualAndTrustCategoryType;
        state.HasCompaniesHouseNumber = hasCompaniesHouseNumber;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.BackPageLink.Should().Be(expectedBackPage);
    }

    [Fact]
    public void OnGet_ShouldSetSupplierHasCompanyHouseNumberToTrue_WhenSupplierHasCompanyHouseNumberIsTrueInSession()
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = true;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.SupplierHasCompanyHouseNumber.Should().Be(true);
    }

    [Fact]
    public void OnGet_ShouldSetSupplierHasCompanyHouseNumberToFalse_WhenSupplierHasCompanyHouseNumberIsFalseInSession()
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = false;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.SupplierHasCompanyHouseNumber.Should().Be(false);
    }

    [Theory]
    [InlineData("ConnectedEntityCompanyRegistrationDate", false)]
    [InlineData("ConnectedEntityCheckAnswersOrganisation", true)]
    public void OnPost_ShouldRedirectToExpectedPage_WhenModelStateIsValid(string expectedRedirectPage, bool redirectToCheckYourAnswer)
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);
        _model.RedirectToCheckYourAnswer = redirectToCheckYourAnswer;

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
        _model.ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares];
        _model.OnPost();

        _sessionMock.Verify(v => v.Set(Session.ConnectedPersonKey,
            It.Is<ConnectedEntityState>(rd =>
                rd.ControlConditions!.Contains(Constants.ConnectedEntityControlCondition.OwnsShares)
            )), Times.Once);
    }

    [Fact]
    public void OnPost_ShouldSetSupplierHasCompanyHouseNumberToTrue_WhenSupplierHasCompanyHouseNumberIsTrueInSession()
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = true;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _model.SupplierHasCompanyHouseNumber.Should().Be(true);
    }

    [Fact]
    public void OnPost_ShouldSetSupplierHasCompanyHouseNumberToFalse_WhenSupplierHasCompanyHouseNumberIsFalseInSession()
    {
        var state = DummyConnectedPersonDetails();

        state.SupplierHasCompanyHouseNumber = false;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _model.SupplierHasCompanyHouseNumber.Should().Be(false);
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
            ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares]
        };

        return connectedPersonDetails;
    }

    private ConnectedEntityState.Address GetDummyAddress()
    {
        return new ConnectedEntityState.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", Country = "United kingdom" };
    }
}