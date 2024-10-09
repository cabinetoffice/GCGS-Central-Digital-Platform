using Amazon;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CompaniesHouse;
using FluentAssertions;
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
        var companyNumber = "12345678";
        var expectedAddress = new RegisteredAddress { AddressLine1 = "123 Test St" };
        _mockConfiguration.Setup(c => c["CompaniesHouse:Url"]).Returns("https://api.companieshouse.gov.uk");
        _mockConfiguration.Setup(c => c["CompaniesHouse:User"]).Returns("testuser");
        _mockConfiguration.Setup(c => c["CompaniesHouse:Password"]).Returns("testpassword");

        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(expectedAddress);

        var (regAddress, httpStatus) = await _companiesHouseApi.GetRegisteredAddress(companyNumber);

        regAddress.Should().NotBeNull();
        expectedAddress.AddressLine1.Should().Match(regAddress!.AddressLine1);
    }

    [Fact]
    public async Task GetProfile_ShouldReturnCompanyProfile()
    {
        var companyNumber = "12345678";
        var expectedProfile = new CompanyProfile { CompanyName = "Test Company" };
        _mockConfiguration.Setup(c => c["CompaniesHouse:Url"]).Returns("https://api.companieshouse.gov.uk");
        _mockConfiguration.Setup(c => c["CompaniesHouse:User"]).Returns("testuser");
        _mockConfiguration.Setup(c => c["CompaniesHouse:Password"]).Returns("testpassword");

        using var httpTest = new HttpTest();
        httpTest.RespondWithJson(expectedProfile);

        var (profile, httpStatus) = await _companiesHouseApi.GetProfile(companyNumber);

        profile.Should().NotBeNull();
        expectedProfile.CompanyName.Should().Match(profile!.CompanyName);
    }
}