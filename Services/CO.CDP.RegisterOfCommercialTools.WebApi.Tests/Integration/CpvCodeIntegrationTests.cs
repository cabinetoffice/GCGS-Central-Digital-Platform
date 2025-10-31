using CO.CDP.RegisterOfCommercialTools.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Integration;

public class CpvCodeIntegrationTests
{
    private readonly HttpClient _client;
    private readonly Mock<ICpvCodeRepository> _mockRepository;

    public CpvCodeIntegrationTests()
    {
        _mockRepository = new Mock<ICpvCodeRepository>();
        SetupMockRepository();

        var factory = new TestWebApplicationFactory<Program>(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                var testConfig = new Dictionary<string, string?>
                {
                    { "AWS:Region", "eu-west-2" },
                    { "AWS:ServiceURL", "http://localhost:4566" },
                    { "AWS:Credentials:AccessKeyId", "test" },
                    { "AWS:Credentials:SecretAccessKey", "test" },
                    { "AWS:CognitoAuthentication:UserPoolId", "test-pool" },
                    { "AWS:CognitoAuthentication:UserPoolClientId", "test-client" },
                    { "AWS:CognitoAuthentication:UserPoolClientSecret", "test-secret" },
                    { "AWS:CognitoAuthentication:Domain", "test-domain" },
                    { "Aws:CloudWatch:LogGroup", "/test/commercial-tools-api" },
                    { "Aws:CloudWatch:LogStream", "test-serilog" }
                };
                AuthenticationTestHelpers.AddApiKeyConfiguration(testConfig);
                config.AddInMemoryCollection(testConfig);
            });
            builder.ConfigureServices((context, services) =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ICpvCodeRepository));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                services.AddSingleton(_mockRepository.Object);
            });
        });

        _client = factory.CreateClient();
        AuthenticationTestHelpers.AddApiKeyAuthentication(_client);
    }

    [Fact]
    public async Task GetRootCodes_ShouldReturnRootCpvCodes()
    {
        var response = await _client.GetAsync("/api/CpvCode/root");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var codes = JsonSerializer.Deserialize<List<CpvCode>>(content, GetJsonOptions());

        codes.Should().NotBeNull();
        codes.Should().HaveCount(3);
        codes![0].Code.Should().Be("03000000");
        codes[0].DescriptionEn.Should().Be("Agricultural products");
        codes[1].Code.Should().Be("09000000");
        codes[2].Code.Should().Be("15000000");

        _mockRepository.Verify(x => x.GetRootCodesAsync(Culture.English), Times.Once);
    }

    [Fact]
    public async Task GetRootCodes_WithWelshCulture_ShouldPassCultureToRepository()
    {
        var response = await _client.GetAsync("/api/CpvCode/root?culture=Welsh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockRepository.Verify(x => x.GetRootCodesAsync(Culture.Welsh), Times.Once);
    }

    [Fact]
    public async Task GetChildren_ShouldReturnChildCodesForParent()
    {
        var response = await _client.GetAsync("/api/CpvCode/03000000/children");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var codes = JsonSerializer.Deserialize<List<CpvCode>>(content, GetJsonOptions());

        codes.Should().NotBeNull();
        codes.Should().HaveCount(2);
        codes![0].Code.Should().Be("03100000");
        codes[0].DescriptionEn.Should().Be("Live animals");
        codes[1].Code.Should().Be("03200000");

        _mockRepository.Verify(x => x.GetChildrenAsync("03000000", Culture.English), Times.Once);
    }

    [Fact]
    public async Task GetChildren_WithWelshCulture_ShouldPassCultureToRepository()
    {
        var response = await _client.GetAsync("/api/CpvCode/03000000/children?culture=Welsh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockRepository.Verify(x => x.GetChildrenAsync("03000000", Culture.Welsh), Times.Once);
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnMatchingCodes()
    {
        var response = await _client.GetAsync("/api/CpvCode/search?query=agricultural");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var codes = JsonSerializer.Deserialize<List<CpvCode>>(content, GetJsonOptions());

        codes.Should().NotBeNull();
        codes.Should().HaveCount(2);
        codes![0].Code.Should().Be("03000000");
        codes[0].DescriptionEn.Should().Contain("Agricultural");

        _mockRepository.Verify(x => x.SearchAsync("agricultural", Culture.English), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/CpvCode/search?query=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithMissingQuery_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/CpvCode/search");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithWhitespaceQuery_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/CpvCode/search?query=   ");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithWelshCulture_ShouldPassCultureToRepository()
    {
        var response = await _client.GetAsync("/api/CpvCode/search?query=cynhyrchion&culture=Welsh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockRepository.Verify(x => x.SearchAsync("cynhyrchion", Culture.Welsh), Times.Once);
    }

    [Fact]
    public async Task GetByCodes_WithValidCodes_ShouldReturnMatchingCodes()
    {
        var codes = new List<string> { "03000000", "09000000" };

        var response = await _client.PostAsJsonAsync("/api/CpvCode/lookup", codes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var cpvCodes = JsonSerializer.Deserialize<List<CpvCode>>(content, GetJsonOptions());

        cpvCodes.Should().NotBeNull();
        cpvCodes.Should().HaveCount(2);
        cpvCodes![0].Code.Should().Be("03000000");
        cpvCodes[1].Code.Should().Be("09000000");

        _mockRepository.Verify(x => x.GetByCodesAsync(It.Is<List<string>>(list =>
            list.Count == 2 && list.Contains("03000000") && list.Contains("09000000"))), Times.Once);
    }

    [Fact]
    public async Task GetByCodes_WithEmptyList_ShouldReturnBadRequest()
    {
        var codes = new List<string>();

        var response = await _client.PostAsJsonAsync("/api/CpvCode/lookup", codes);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByCodes_WithNullList_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/CpvCode/lookup", (List<string>?)null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetHierarchy_WithValidCode_ShouldReturnHierarchy()
    {
        var response = await _client.GetAsync("/api/CpvCode/03110000/hierarchy");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var hierarchy = JsonSerializer.Deserialize<List<CpvCode>>(content, GetJsonOptions());

        hierarchy.Should().NotBeNull();
        hierarchy.Should().HaveCount(3);
        hierarchy![0].Level.Should().Be(1);
        hierarchy[1].Level.Should().Be(2);
        hierarchy[2].Level.Should().Be(3);

        _mockRepository.Verify(x => x.GetHierarchyAsync("03110000"), Times.Once);
    }

    [Fact]
    public async Task GetHierarchy_WithNonExistentCode_ShouldReturnEmptyList()
    {
        _mockRepository.Setup(x => x.GetHierarchyAsync("99999999"))
                     .ReturnsAsync(new List<CpvCode>());

        var response = await _client.GetAsync("/api/CpvCode/99999999/hierarchy");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var hierarchy = JsonSerializer.Deserialize<List<CpvCode>>(content, GetJsonOptions());

        hierarchy.Should().NotBeNull();
        hierarchy.Should().BeEmpty();
    }

    [Fact]
    public async Task AllEndpoints_ShouldReturnJsonContentType()
    {
        var endpoints = new[]
        {
            "/api/CpvCode/root",
            "/api/CpvCode/03000000/children",
            "/api/CpvCode/search?query=test",
            "/api/CpvCode/03110000/hierarchy"
        };

        foreach (var endpoint in endpoints)
        {
            var response = await _client.GetAsync(endpoint);

            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }
    }

    [Fact]
    public async Task PostLookup_ShouldReturnJsonContentType()
    {
        var codes = new List<string> { "03000000" };
        var response = await _client.PostAsJsonAsync("/api/CpvCode/lookup", codes);

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    private void SetupMockRepository()
    {
        var rootCodes = new List<CpvCode>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1, IsActive = true },
            new() { Code = "09000000", DescriptionEn = "Petroleum products", DescriptionCy = "Cynhyrchion petroleum", Level = 1, IsActive = true },
            new() { Code = "15000000", DescriptionEn = "Food products", DescriptionCy = "Cynhyrchion bwyd", Level = 1, IsActive = true }
        };

        var childCodes = new List<CpvCode>
        {
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", ParentCode = "03000000", Level = 2, IsActive = true },
            new() { Code = "03200000", DescriptionEn = "Cereal crops", DescriptionCy = "Cnydau grawn", ParentCode = "03000000", Level = 2, IsActive = true }
        };

        var searchResults = new List<CpvCode>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1, IsActive = true },
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", ParentCode = "03000000", Level = 2, IsActive = true }
        };

        var hierarchyCodes = new List<CpvCode>
        {
            new() { Code = "03000000", DescriptionEn = "Agricultural products", DescriptionCy = "Cynhyrchion amaethyddol", Level = 1, IsActive = true },
            new() { Code = "03100000", DescriptionEn = "Live animals", DescriptionCy = "Anifeiliaid byw", ParentCode = "03000000", Level = 2, IsActive = true },
            new() { Code = "03110000", DescriptionEn = "Live bovine animals", DescriptionCy = "Anifeiliaid gwartheg byw", ParentCode = "03100000", Level = 3, IsActive = true }
        };

        _mockRepository.Setup(x => x.GetRootCodesAsync(It.IsAny<Culture>()))
                      .ReturnsAsync(rootCodes);

        _mockRepository.Setup(x => x.GetChildrenAsync(It.IsAny<string>(), It.IsAny<Culture>()))
                      .ReturnsAsync(childCodes);

        _mockRepository.Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<Culture>()))
                      .ReturnsAsync(searchResults);

        _mockRepository.Setup(x => x.GetByCodesAsync(It.IsAny<List<string>>()))
                      .ReturnsAsync((List<string> codes) => rootCodes.Where(c => codes.Contains(c.Code)).ToList());

        _mockRepository.Setup(x => x.GetHierarchyAsync(It.IsAny<string>()))
                      .ReturnsAsync(hierarchyCodes);
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }
}