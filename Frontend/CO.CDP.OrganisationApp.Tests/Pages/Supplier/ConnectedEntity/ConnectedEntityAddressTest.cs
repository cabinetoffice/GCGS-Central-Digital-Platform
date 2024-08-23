using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityAddressTest
{
    private readonly ConnectedEntityAddressModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityAddressTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityAddressModel(_sessionMock.Object);
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
        _model.ConnectedEntityId = _entityId;
        state.OrganisationName = "Org_name";
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
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
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityPostalSameAsRegisteredAddress", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityPostalSameAsRegisteredAddress", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityPostalSameAsRegisteredAddress", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityCompanyQuestion", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityPostalSameAsRegisteredAddress", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl)]
    [InlineData(Constants.AddressType.Postal, false, "ConnectedEntityLawRegister", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany)]
    [InlineData(Constants.AddressType.Postal, false, "ConnectedEntityLawRegister", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities)]
    [InlineData(Constants.AddressType.Postal, false, "ConnectedEntityCompanyQuestion", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany)]
    [InlineData(Constants.AddressType.Postal, false, "ConnectedEntityCompanyQuestion", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl)]
    [InlineData(Constants.AddressType.Postal, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.RegisteredCompany)]
    [InlineData(Constants.AddressType.Postal, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities)]
    [InlineData(Constants.AddressType.Postal, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany)]
    [InlineData(Constants.AddressType.Postal, true, "ConnectedEntityCheckAnswersOrganisation", Constants.ConnectedEntityType.Organisation, "organisation-name", Constants.ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityControlCondition", Constants.ConnectedEntityType.Individual, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.Individual, "director-residency", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityControlCondition", Constants.ConnectedEntityType.Individual, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.Individual, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.Individual, "director-residency", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.Individual, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityControlCondition", Constants.ConnectedEntityType.TrustOrTrustee, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.TrustOrTrustee, "director-residency", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust)]
    [InlineData(Constants.AddressType.Registered, false, "ConnectedEntityControlCondition", Constants.ConnectedEntityType.TrustOrTrustee, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.TrustOrTrustee, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.TrustOrTrustee, "director-residency", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust)]
    [InlineData(Constants.AddressType.Registered, true, "ConnectedEntityCheckAnswersIndividualOrTrust", Constants.ConnectedEntityType.TrustOrTrustee, "individual-psc-details", null, Constants.ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust)]

    public void OnPost_ShouldRedirectToConnectedEntityCategoryPage
        (Constants.AddressType addressType,
        bool redirectToCheckYourAnswer,
        string expectedRedirectPage,
        Constants.ConnectedEntityType connectedEntityType,
        string expectedBackPage,
        Constants.ConnectedEntityOrganisationCategoryType? organisationCategoryType = null,
        Constants.ConnectedEntityIndividualAndTrustCategoryType? individualAndTrustCategoryType = null)
    {
        var state = DummyConnectedPersonDetails();

        if (addressType == Constants.AddressType.Registered)
        {
            state.RegisteredAddress = GetDummyAddress();
            state.PostalAddress = null;
        }
        else
        {
            state.PostalAddress = GetDummyAddress();
            state.RegisteredAddress = GetDummyAddress();
        }

        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        state.ConnectedEntityIndividualAndTrustCategoryType = individualAndTrustCategoryType;
        _model.RedirectToCheckYourAnswer = redirectToCheckYourAnswer;
        _model.ConnectedEntityType = connectedEntityType;
        _model.AddressType = addressType;

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);

        _model.BackPageName.Should().Contain(expectedBackPage);
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
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678",
            ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares],
            RegistrationDate = new DateTimeOffset(2011, 7, 15, 0, 0, 0, TimeSpan.FromHours(0))
        };

        return connectedPersonDetails;
    }

    private ConnectedEntityState.Address GetDummyAddress()
    {
        return new ConnectedEntityState.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", Country = "GB" };
    }
}