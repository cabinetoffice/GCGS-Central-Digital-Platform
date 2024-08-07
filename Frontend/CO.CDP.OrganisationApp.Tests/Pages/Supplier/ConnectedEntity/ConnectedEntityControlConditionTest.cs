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
    [InlineData("overseas-company-question", ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, null)]
    [InlineData("company-question", ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, true)]
    [InlineData("company-question", ConnectedEntityOrganisationCategoryType.RegisteredCompany, null)]
    public void OnGet_ShouldReturnBackPageResult(
            string expectedBackPage,
            ConnectedEntityOrganisationCategoryType categoryType,
            bool? hasCompaniesHouseNumber)
    {
        var state = DummyConnectedPersonDetails();

        state.ConnectedEntityOrganisationCategoryType = categoryType;
        state.HasCompaniesHouseNumber = hasCompaniesHouseNumber;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.BackPageLink.Should().Contain(expectedBackPage);
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

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be(expectedRedirectPage);
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
            ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares]
        };

        return connectedPersonDetails;
    }
}