using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Moq;
using DevolvedRegulation = CO.CDP.OrganisationApp.Constants.DevolvedRegulation;
using OrganisationType = CO.CDP.OrganisationApp.Constants.OrganisationType;

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
    [InlineData(RegistrationStepModel.SupplierOrganisationTypePage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK")]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", null, null, null, new[] { OperationType.SmallOrMediumSized })]
    [InlineData(RegistrationStepModel.BuyerOrganisationTypePage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK")]
    [InlineData(RegistrationStepModel.BuyerDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1")]
    [InlineData(RegistrationStepModel.BuyerSelectDevolvedRegulationPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", true)]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", false)]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, OrganisationType.Buyer, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK", "type-1", true, new[] { DevolvedRegulation.NorthernIreland })]
    public void ValidateStep_ShouldValidateType(string page, OrganisationType? organisationType = null, string? organisationScheme = null,
        string? organisationName = null, string? organisationEmail = null, string? addressLine1 = null, string? city = null,
        string? postcode = null, string? country = null,
        string? buyerOrganisationType = null, bool? devolved = null, IEnumerable<DevolvedRegulation>? regulations = null,
        IEnumerable<OperationType>? supplierOperationTypes = null)
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
            OrganisationCountryCode = country ?? "UK",
            BuyerOrganisationType = buyerOrganisationType,
            Devolved = devolved,
            Regulations = regulations == null ? [] : regulations.ToList(),
            SupplierOrganisationOperationTypes = supplierOperationTypes == null ? [] : supplierOperationTypes.ToList()
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
    [InlineData(RegistrationStepModel.SupplierOrganisationTypePage, RegistrationStepModel.OrganisationAddressPage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com")]
    [InlineData(RegistrationStepModel.SupplierOrganisationTypePage, RegistrationStepModel.OrganisationTypePage, OrganisationType.Buyer)]
    [InlineData(RegistrationStepModel.OrganisationSummaryPage, RegistrationStepModel.SupplierOrganisationTypePage, OrganisationType.Supplier, "Test", "Test Org", "test@example.com", "add 1", "city", "post code", "UK")]
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
        string? buyerOrganisationType = null, bool? devolved = null, IEnumerable<DevolvedRegulation>? regulations = null,
        IEnumerable<OperationType>? supplierOperationTypes = null)
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
            OrganisationCountryCode = country ?? "UK",
            BuyerOrganisationType = buyerOrganisationType,
            Devolved = devolved,
            Regulations = regulations == null ? [] : regulations.ToList(),
            SupplierOrganisationOperationTypes = supplierOperationTypes == null ? [] : supplierOperationTypes.ToList()
        });

        var model = CreateModel(currentPage);
        var result = model.ValidateStep();

        result.Should().BeFalse();
        model.ToRedirectPageUrl.Should().Be(expectedRedirectPage);
    }

    [Fact]
    public void ValidateStep_SupplierOrganisationTypePage_WhenNotSupplier_ShouldRedirectToOrganisationType()
    {
        SetupRegistrationDetails(new RegistrationDetails
        {
            OrganisationType = OrganisationType.Buyer,
            OrganisationScheme = "Test",
            OrganisationName = "Test Org",
            OrganisationEmailAddress = "test@example.com",
            OrganisationAddressLine1 = "add 1",
            OrganisationCityOrTown = "city",
            OrganisationPostcode = "post code",
            OrganisationCountryCode = "UK"
        });

        var model = CreateModel(RegistrationStepModel.SupplierOrganisationTypePage);
        var result = model.ValidateStep();

        result.Should().BeFalse();
        model.ToRedirectPageUrl.Should().Be(RegistrationStepModel.OrganisationTypePage);
    }

    [Fact]
    public void ValidateStep_OrganisationSummaryPage_WhenSupplierWithOperationTypes_ShouldPass()
    {
        SetupRegistrationDetails(new RegistrationDetails
        {
            OrganisationType = OrganisationType.Supplier,
            OrganisationScheme = "Test",
            OrganisationName = "Test Org",
            OrganisationEmailAddress = "test@example.com",
            OrganisationAddressLine1 = "add 1",
            OrganisationCityOrTown = "city",
            OrganisationPostcode = "post code",
            OrganisationCountryCode = "UK",
            SupplierOrganisationOperationTypes = [OperationType.SmallOrMediumSized]
        });

        var model = CreateModel(RegistrationStepModel.OrganisationSummaryPage);
        var result = model.ValidateStep();

        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateStep_OrganisationSummaryPage_WhenSupplierWithoutOperationTypes_ShouldRedirectToSupplierOrganisationType()
    {
        SetupRegistrationDetails(new RegistrationDetails
        {
            OrganisationType = OrganisationType.Supplier,
            OrganisationScheme = "Test",
            OrganisationName = "Test Org",
            OrganisationEmailAddress = "test@example.com",
            OrganisationAddressLine1 = "add 1",
            OrganisationCityOrTown = "city",
            OrganisationPostcode = "post code",
            OrganisationCountryCode = "UK",
            SupplierOrganisationOperationTypes = []
        });

        var model = CreateModel(RegistrationStepModel.OrganisationSummaryPage);
        var result = model.ValidateStep();

        result.Should().BeFalse();
        model.ToRedirectPageUrl.Should().Be(RegistrationStepModel.SupplierOrganisationTypePage);
    }

    [Fact]
    public void ValidateStep_OrganisationSummaryPage_WhenSupplierWithNullOperationTypes_ShouldRedirectToSupplierOrganisationType()
    {
        var details = new RegistrationDetails
        {
            OrganisationType = OrganisationType.Supplier,
            OrganisationScheme = "Test",
            OrganisationName = "Test Org",
            OrganisationEmailAddress = "test@example.com",
            OrganisationAddressLine1 = "add 1",
            OrganisationCityOrTown = "city",
            OrganisationPostcode = "post code",
            OrganisationCountryCode = "UK"
        };
        
        details.SupplierOrganisationOperationTypes = [];

        SetupRegistrationDetails(details);

        var model = CreateModel(RegistrationStepModel.OrganisationSummaryPage);
        var result = model.ValidateStep();

        result.Should().BeFalse();
        model.ToRedirectPageUrl.Should().Be(RegistrationStepModel.SupplierOrganisationTypePage);
    }

    private TestRegistrationStepModel CreateModel(string page)
    {
        return new TestRegistrationStepModel(sessionMock.Object, page);
    }

    private void SetupRegistrationDetails(RegistrationDetails details)
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(details);
    }

    private class TestRegistrationStepModel(ISession session, string currentPage) : RegistrationStepModel(session)
    {
        public override string CurrentPage { get; } = currentPage;
    }
}