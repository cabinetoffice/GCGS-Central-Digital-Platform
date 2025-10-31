using CO.CDP.RegisterOfCommercialTools.Persistence;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace CO.CDP.RegisterOfCommercialTools.WebApi.Tests.Integration;

public class NutsCodeIntegrationTests
{
    private readonly HttpClient _client;
    private readonly Mock<INutsCodeRepository> _mockRepository;

    public NutsCodeIntegrationTests()
    {
        _mockRepository = new Mock<INutsCodeRepository>();
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
            builder.ConfigureServices((_, services) =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(INutsCodeRepository));
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
    public async Task GetRootCodes_ShouldReturnRootNutsCodes()
    {
        var response = await _client.GetAsync("/api/NutsCode/root");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var codes = JsonSerializer.Deserialize<List<NutsCode>>(content, GetJsonOptions());

        codes.Should().NotBeNull();
        codes.Should().HaveCount(3);
        codes![0].Code.Should().Be("UKC");
        codes[0].DescriptionEn.Should().Be("North East (England)");
        codes[1].Code.Should().Be("UKD");
        codes[2].Code.Should().Be("UKE");

        _mockRepository.Verify(x => x.GetRootCodesAsync(Culture.English), Times.Once);
    }

    [Fact]
    public async Task GetRootCodes_WithWelshCulture_ShouldPassCultureToRepository()
    {
        var response = await _client.GetAsync("/api/NutsCode/root?culture=Welsh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockRepository.Verify(x => x.GetRootCodesAsync(Culture.Welsh), Times.Once);
    }

    [Fact]
    public async Task GetChildren_ShouldReturnChildCodesForParent()
    {
        var response = await _client.GetAsync("/api/NutsCode/UKC/children");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var codes = JsonSerializer.Deserialize<List<NutsCode>>(content, GetJsonOptions());

        codes.Should().NotBeNull();
        codes.Should().HaveCount(2);
        codes![0].Code.Should().Be("UKC1");
        codes[0].DescriptionEn.Should().Be("Tees Valley and Durham");
        codes[1].Code.Should().Be("UKC2");

        _mockRepository.Verify(x => x.GetChildrenAsync("UKC", Culture.English), Times.Once);
    }

    [Fact]
    public async Task GetChildren_WithWelshCulture_ShouldPassCultureToRepository()
    {
        var response = await _client.GetAsync("/api/NutsCode/UKC/children?culture=Welsh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockRepository.Verify(x => x.GetChildrenAsync("UKC", Culture.Welsh), Times.Once);
    }

    [Fact]
    public async Task Search_WithValidQuery_ShouldReturnMatchingCodes()
    {
        var response = await _client.GetAsync("/api/NutsCode/search?query=north");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var codes = JsonSerializer.Deserialize<List<NutsCode>>(content, GetJsonOptions());

        codes.Should().NotBeNull();
        codes.Should().HaveCount(2);
        codes![0].Code.Should().Be("UKC");
        codes[0].DescriptionEn.Should().Contain("North");

        _mockRepository.Verify(x => x.SearchAsync("north", Culture.English), Times.Once);
    }

    [Fact]
    public async Task Search_WithEmptyQuery_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/NutsCode/search?query=");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithMissingQuery_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/NutsCode/search");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithWhitespaceQuery_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/NutsCode/search?query=   ");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Search_WithWelshCulture_ShouldPassCultureToRepository()
    {
        var response = await _client.GetAsync("/api/NutsCode/search?query=gogledd&culture=Welsh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        _mockRepository.Verify(x => x.SearchAsync("gogledd", Culture.Welsh), Times.Once);
    }

    [Fact]
    public async Task GetByCodes_WithValidCodes_ShouldReturnMatchingCodes()
    {
        var codes = new List<string> { "UKC", "UKD" };

        var response = await _client.PostAsJsonAsync("/api/NutsCode/lookup", codes);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var nutsCodes = JsonSerializer.Deserialize<List<NutsCode>>(content, GetJsonOptions());

        nutsCodes.Should().NotBeNull();
        nutsCodes.Should().HaveCount(2);
        nutsCodes![0].Code.Should().Be("UKC");
        nutsCodes[1].Code.Should().Be("UKD");

        _mockRepository.Verify(x => x.GetByCodesAsync(It.Is<List<string>>(list =>
            list.Count == 2 && list.Contains("UKC") && list.Contains("UKD"))), Times.Once);
    }

    [Fact]
    public async Task GetByCodes_WithEmptyList_ShouldReturnBadRequest()
    {
        var codes = new List<string>();

        var response = await _client.PostAsJsonAsync("/api/NutsCode/lookup", codes);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetByCodes_WithNullList_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/NutsCode/lookup", (List<string>?)null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetHierarchy_WithValidCode_ShouldReturnHierarchy()
    {
        var response = await _client.GetAsync("/api/NutsCode/UKC11/hierarchy");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var hierarchy = JsonSerializer.Deserialize<List<NutsCode>>(content, GetJsonOptions());

        hierarchy.Should().NotBeNull();
        hierarchy.Should().HaveCount(3);
        hierarchy![0].Level.Should().Be(1);
        hierarchy[1].Level.Should().Be(2);
        hierarchy[2].Level.Should().Be(3);

        _mockRepository.Verify(x => x.GetHierarchyAsync("UKC11"), Times.Once);
    }

    [Fact]
    public async Task GetHierarchy_WithNonExistentCode_ShouldReturnEmptyList()
    {
        _mockRepository.Setup(x => x.GetHierarchyAsync("INVALID"))
                     .ReturnsAsync(new List<NutsCode>());

        var response = await _client.GetAsync("/api/NutsCode/INVALID/hierarchy");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        var hierarchy = JsonSerializer.Deserialize<List<NutsCode>>(content, GetJsonOptions());

        hierarchy.Should().NotBeNull();
        hierarchy.Should().BeEmpty();
    }

    [Fact]
    public async Task AllEndpoints_ShouldReturnJsonContentType()
    {
        var endpoints = new[]
        {
            "/api/NutsCode/root",
            "/api/NutsCode/UKC/children",
            "/api/NutsCode/search?query=test",
            "/api/NutsCode/UKC11/hierarchy"
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
        var codes = new List<string> { "UKC" };
        var response = await _client.PostAsJsonAsync("/api/NutsCode/lookup", codes);

        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
    }

    private void SetupMockRepository()
    {
        var rootCodes = new List<NutsCode>
        {
            new() { Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)", Level = 1, IsActive = true, IsSelectable = true },
            new() { Code = "UKD", DescriptionEn = "North West (England)", DescriptionCy = "Gogledd Orllewin (Lloegr)", Level = 1, IsActive = true, IsSelectable = true },
            new() { Code = "UKE", DescriptionEn = "Yorkshire and The Humber", DescriptionCy = "Yorkshire a'r Humber", Level = 1, IsActive = true, IsSelectable = true }
        };

        var childCodes = new List<NutsCode>
        {
            new() { Code = "UKC1", DescriptionEn = "Tees Valley and Durham", DescriptionCy = "Cwm Tees a Durham", ParentCode = "UKC", Level = 2, IsActive = true, IsSelectable = true },
            new() { Code = "UKC2", DescriptionEn = "Northumberland and Tyne and Wear", DescriptionCy = "Northumberland a Tyne a Wear", ParentCode = "UKC", Level = 2, IsActive = true, IsSelectable = true }
        };

        var searchResults = new List<NutsCode>
        {
            new() { Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)", Level = 1, IsActive = true, IsSelectable = true },
            new() { Code = "UKD", DescriptionEn = "North West (England)", DescriptionCy = "Gogledd Orllewin (Lloegr)", Level = 1, IsActive = true, IsSelectable = true }
        };

        var hierarchyCodes = new List<NutsCode>
        {
            new() { Code = "UKC", DescriptionEn = "North East (England)", DescriptionCy = "Gogledd Ddwyrain (Lloegr)", Level = 1, IsActive = true, IsSelectable = true },
            new() { Code = "UKC1", DescriptionEn = "Tees Valley and Durham", DescriptionCy = "Cwm Tees a Durham", ParentCode = "UKC", Level = 2, IsActive = true, IsSelectable = true },
            new() { Code = "UKC11", DescriptionEn = "Hartlepool and Stockton-on-Tees", DescriptionCy = "Hartlepool a Stockton-on-Tees", ParentCode = "UKC1", Level = 3, IsActive = true, IsSelectable = true }
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