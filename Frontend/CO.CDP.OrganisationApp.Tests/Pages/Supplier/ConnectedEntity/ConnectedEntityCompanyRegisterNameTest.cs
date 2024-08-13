using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCompanyRegisterNameTest
{
    private readonly ConnectedEntityCompanyRegisterNameModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityCompanyRegisterNameTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityCompanyRegisterNameModel(_sessionMock.Object);
        _model.Id = _organisationId;
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
    public void OnGet_ShouldReturnPageResultAndSetRegisterNameInput_WhenOtherSelected()
    {
        var state = DummyConnectedPersonDetails();
        _model.ConnectedEntityId = _entityId;
        state.RegisterName = "reg_name";
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.RegisterName.Should().Be("other");
        _model.RegisterNameInput.Should().Be("reg_name");
    }

    [Fact]
    public void OnGet_ShouldReturnPageResultAndUseCompaniesHouse_WhenCompaniesHouseSelected()
    {
        var state = DummyConnectedPersonDetails();
        _model.ConnectedEntityId = _entityId;
        state.RegisterName = "Companies House";
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.RegisterName.Should().Be("Companies House");
        _model.RegisterNameInput.Should().Be(null);
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntityQuestion_WhenSessionStateIsNull()
    {
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns((ConnectedEntityState?)null);

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");
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

    [Theory]
    [InlineData(Constants.ConnectedEntityType.Organisation, Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany, false,"ConnectedEntityCheckAnswersOrganisation", "date-registered-question")]
    [InlineData(Constants.ConnectedEntityType.Organisation, Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany, true,"ConnectedEntityCheckAnswersOrganisation", "date-registered")]
    [InlineData(Constants.ConnectedEntityType.Organisation, Constants.ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, false, "ConnectedEntityLegalFormQuestion", "date-registered-question")]
    public void OnPost_ShouldRedirectToConnectedEntityCategoryPageOrganisation(
        Constants.ConnectedEntityType connectedEntityType,
        Constants.ConnectedEntityOrganisationCategoryType orgCategoryType,
        bool supplierHasCompanyNumber,
        string expectedRedirectPage,
        string expectedBackPageLink
        )
    {
        var state = DummyConnectedPersonDetails();
        _model.RegisterName = "reg_name";
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = orgCategoryType;
        state.SupplierHasCompanyHouseNumber = supplierHasCompanyNumber;

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);

        _model.BackPageLink.Should().Be(expectedBackPageLink);
    }

    [Theory]
    [InlineData(Constants.ConnectedEntityType.Individual, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, false,"ConnectedEntityCheckAnswersIndividualOrTrust", "date-registered-question")]
    [InlineData(Constants.ConnectedEntityType.TrustOrTrustee, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, false,"ConnectedEntityCheckAnswersIndividualOrTrust", "date-registered-question")]
    [InlineData(Constants.ConnectedEntityType.Individual, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual, true,"ConnectedEntityCheckAnswersIndividualOrTrust", "date-registered")]
    [InlineData(Constants.ConnectedEntityType.TrustOrTrustee, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust, true,"ConnectedEntityCheckAnswersIndividualOrTrust", "date-registered")]
    public void OnPost_ShouldRedirectToConnectedEntityCategoryPageIndividualTrust(
        Constants.ConnectedEntityType connectedEntityType,
        Constants.ConnectedEntityIndividualAndTrustCategoryType individualOTrustCategoryType,
        bool supplierHasCompanyNumber,
        string expectedRedirectPage,
        string expectedBackPageLink
        )
    {
        var state = DummyConnectedPersonDetails();
        _model.RegisterName = "reg_name";
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityIndividualAndTrustCategoryType = individualOTrustCategoryType;
        state.SupplierHasCompanyHouseNumber = supplierHasCompanyNumber;

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);

        _model.BackPageLink.Should().Be(expectedBackPageLink);
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        var state = DummyConnectedPersonDetails();

        _model.RegisterName = "reg_name";

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(s => s.Set(Session.ConnectedPersonKey, It.Is<ConnectedEntityState>(st => st.ConnectedEntityType == Constants.ConnectedEntityType.Organisation)), Times.Once);

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
            RegisterName = "reg_name"
        };

        return connectedPersonDetails;
    }
}