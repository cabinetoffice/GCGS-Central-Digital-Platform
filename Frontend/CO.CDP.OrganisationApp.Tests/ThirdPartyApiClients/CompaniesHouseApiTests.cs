using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using Flurl.Http.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace CO.CDP.OrganisationApp.ThirdPartyApiClients;

public class CompaniesHouseApiTest
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly Mock<ILogger<CompaniesHouseApi>> _mockLogger;
    private readonly CompaniesHouseApi _companiesHouseApi;

    public CompaniesHouseApiTest()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<CompaniesHouseApi>>();
        _companiesHouseApi = new CompaniesHouseApi(_mockConfiguration.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRegisteredAddress_ShouldReturnRegisteredAddress()
    {
        // Arrange
        var companyNumber = "12345678";
        var expectedAddress = new RegisteredAddress { AddressLine1 = "123 Test St" };
        _mockConfiguration.Setup(c => c["CompaniesHouse:Url"]).Returns("https://api.companieshouse.gov.uk");
        _mockConfiguration.Setup(c => c["CompaniesHouse:User"]).Returns("testuser");
        _mockConfiguration.Setup(c => c["CompaniesHouse:Password"]).Returns("testpassword");

        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(expectedAddress);

        // Act
        var result = await _companiesHouseApi.GetRegisteredAddress(companyNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedAddress.AddressLine1, result.AddressLine1);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnCompanyProfile()
    {
        // Arrange
        var companyNumber = "12345678";
        var expectedProfile = new CompanyProfile { CompanyName = "Test Company" };
        _mockConfiguration.Setup(c => c["CompaniesHouse:Url"]).Returns("https://api.companieshouse.gov.uk");
        _mockConfiguration.Setup(c => c["CompaniesHouse:User"]).Returns("testuser");
        _mockConfiguration.Setup(c => c["CompaniesHouse:Password"]).Returns("testpassword");

        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(expectedProfile);

        // Act
        var result = await _companiesHouseApi.GetProfile(companyNumber);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedProfile.CompanyName, result.CompanyName);
    }
}