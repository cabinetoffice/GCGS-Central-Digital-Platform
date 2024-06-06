using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class RegistrationStepModelTest
{
    private readonly Mock<ISession> sessionMock;

    public RegistrationStepModelTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey)).Returns(new UserDetails { UserUrn = "test_urn" });
    }

    [Theory]
    [InlineData(RegistrationStepModel.OrganisationTypePage)]
    [InlineData(RegistrationStepModel.OrganisationHasCompanyHouseNumberPage, OrganisationType.Supplier)]
    [InlineData(RegistrationStepModel.OrganisationIdentifierPage, OrganisationType.Supplier)]
    [InlineData(RegistrationStepModel.OrganisationNamePage, OrganisationType.Supplier, "Test")]
    [InlineData(RegistrationStepModel.OrganisationEmailPage, OrganisationType.Supplier, "Test", "Test Org")]
    [InlineData(RegistrationStepModel.OrganisationAddressPage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com")]
    [InlineData(RegistrationStepModel.OrganisationNonUKAddressPage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com")]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK")]
    [InlineData(RegistrationStepModel.BuyerOrganisationTypePage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK")]
    [InlineData(RegistrationStepModel.BuyerDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1")]
    [InlineData(RegistrationStepModel.BuyerSelectDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", true)]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", false)]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", true, new[] { DevolvedRegulation.NorthernIreland })]
    public void ValidateStep_ShouldValidateType(string page, OrganisationType? organisationType = null, string? organisationScheme = null,
        string? organisationName = null, string? organisationEmail = null, string? addressLine1 = null, string? city = null,
        string? postcode = null, string? country = null,
        string? buyerOrganisationType = null, bool? devolved = null, IEnumerable<DevolvedRegulation>? regulations = null)
    {
        SetupRegistrationDetails(new RegistrationDetails
        {
            OrganisationType = organisationType,
            OrganisationScheme = organisationScheme,
            OrganisationName = organisationName,
            OrganisationEmailAddress = organisationEmail,
            OrganisationAddressLine1 = addressLine1,
            OrganisationCityOrTown = city,
            OrganisationPostcode = postcode,
            OrganisationCountry = country ?? "UK",
            BuyerOrganisationType = buyerOrganisationType,
            Devolved = devolved,
            Regulations = regulations == null ? [] : regulations.ToList()
        });

        var model = CreateModel(page);
        var result = model.ValidateStep();

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(RegistrationStepModel.OrganisationHasCompanyHouseNumberPage, RegistrationStepModel.OrganisationTypePage)]
    [InlineData(RegistrationStepModel.OrganisationIdentifierPage, RegistrationStepModel.OrganisationTypePage)]
    [InlineData(RegistrationStepModel.OrganisationNamePage, RegistrationStepModel.OrganisationHasCompanyHouseNumberPage, OrganisationType.Buyer)]
    [InlineData(RegistrationStepModel.OrganisationEmailPage, RegistrationStepModel.OrganisationNamePage, OrganisationType.Supplier, "Test")]
    [InlineData(RegistrationStepModel.OrganisationAddressPage, RegistrationStepModel.OrganisationEmailPage, OrganisationType.Supplier, "Test", "Test Org")]
    [InlineData(RegistrationStepModel.OrganisationNonUKAddressPage, RegistrationStepModel.OrganisationEmailPage, OrganisationType.Supplier, "Test", "Test Org")]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, RegistrationStepModel.OrganisationAddressPage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com")]
    [InlineData(RegistrationStepModel.BuyerOrganisationTypePage, RegistrationStepModel.OrganisationAddressPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com")]
    [InlineData(RegistrationStepModel.BuyerOrganisationTypePage, RegistrationStepModel.OrganisationTypePage, OrganisationType.Supplier)]
    [InlineData(RegistrationStepModel.BuyerDevolvedRegulationPage, RegistrationStepModel.BuyerOrganisationTypePage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK")]
    [InlineData(RegistrationStepModel.BuyerSelectDevolvedRegulationPage, RegistrationStepModel.BuyerDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1")]
    [InlineData(RegistrationStepModel.BuyerSelectDevolvedRegulationPage, RegistrationStepModel.BuyerDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", false)]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, RegistrationStepModel.BuyerDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1")]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, RegistrationStepModel.BuyerDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", true)]
    public void ValidateStep_ShouldRedirectToExpectedPage(string currentPage, string expectedRedirectPage, OrganisationType? organisationType = null,
        string? organisationScheme = null, string? organisationName = null, string? organisationEmail = null, string? addressLine1 = null,
        string? city = null, string? postcode = null, string? country = null,
        string? buyerOrganisationType = null, bool? devolved = null, IEnumerable<DevolvedRegulation>? regulations = null)
    {
        SetupRegistrationDetails(new RegistrationDetails
        {
            OrganisationType = organisationType,
            OrganisationScheme = organisationScheme,
            OrganisationName = organisationName,
            OrganisationEmailAddress = organisationEmail,
            OrganisationAddressLine1 = addressLine1,
            OrganisationCityOrTown = city,
            OrganisationPostcode = postcode,
            OrganisationCountry = country ?? "UK",
            BuyerOrganisationType = buyerOrganisationType,
            Devolved = devolved,
            Regulations = regulations == null ? [] : regulations.ToList()
        });

        var model = CreateModel(currentPage);
        var result = model.ValidateStep();

        result.Should().BeFalse();
        model.ToRedirectPageUrl.Should().Be(expectedRedirectPage);
    }

    private TestRegistrationStepModel CreateModel(string page)
    {
        return new TestRegistrationStepModel(sessionMock.Object, page);
    }

    private void SetupRegistrationDetails(RegistrationDetails details)
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(details);
    }

    private class TestRegistrationStepModel(ISession session, string currentPage) : RegistrationStepModel
    {
        public override string CurrentPage { get; } = currentPage;
        public override ISession SessionContext { get; } = session;
    }
}