using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityLawRegisterTest
{
    private readonly ConnectedEntityLawRegisterModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityLawRegisterTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityLawRegisterModel(_sessionMock.Object);
        _model.Id = _organisationId;
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntitySupplierCompanyQuestion_WhenModelStateIsInvalid()
    {
        ConnectedEntityState? state = null;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");
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
        _model.LegalForm.Should().Be("Legal-Form");
        _model.LawRegistered.Should().Be("Law-Registered");
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntitySupplierCompanyQuestion_WhenSessionStateIsNull()
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
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "postal-address-same-as-registered/")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "postal-address-same-as-registered/")]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "Postal-address/uk/", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "Postal-address/uk/", false)]
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
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityCompanyQuestion", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCompanyQuestion", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.RegisteredCompany, "ConnectedEntityCheckAnswersOrganisation", true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities, "ConnectedEntityCheckAnswersOrganisation", true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityCheckAnswersOrganisation", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl, "ConnectedEntityCheckAnswersOrganisation", true)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany, "", false)]
    [InlineData(ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "", false)]
    public void OnPost_ShouldRedirectToExpectedPage(
        ConnectedEntityType connectedEntityType,
        ConnectedEntityOrganisationCategoryType? organisationCategoryType,
        string expectedRedirectPage,
        bool redirectToCheckYourAnswer)
    {
        var state = DummyConnectedPersonDetails();
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        _model.RedirectToCheckYourAnswer = redirectToCheckYourAnswer;

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
            LegalForm = "Legal-Form",
            LawRegistered = "Law-Registered",
            RegisteredAddress = GetDummyAddress(),
            PostalAddress = GetDummyAddress(),
        };

        return connectedPersonDetails;
    }

    private ConnectedEntityState.Address GetDummyAddress()
    {
        return new ConnectedEntityState.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", Country = "United kingdom" };
    }
}