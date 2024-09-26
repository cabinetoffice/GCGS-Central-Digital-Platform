using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Organisation.WebApi.Tests.Api;

public class SupplierInformationEndpointsTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<Guid, SupplierInformation?>> _getSupplierInformationUseCase = new();
    private readonly Mock<IUseCase<(Guid, UpdateSupplierInformation), bool>> _updatesSupplierInformationUseCase = new();

    public SupplierInformationEndpointsTests()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped(_ => _getSupplierInformationUseCase.Object);
                services.AddScoped(_ => _updatesSupplierInformationUseCase.Object);
            });
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetSupplierInformation_ValidOrganisationId_ReturnsOk()
    {
        var organisationId = Guid.NewGuid();
        var supplierInformation = new SupplierInformation { OrganisationName = "FakeOrg" };
        _getSupplierInformationUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync(supplierInformation);

        var returnedSupplierInformation = await _httpClient.GetFromJsonAsync<SupplierInformation>(
            $"/organisations/{organisationId}/supplier-information");

        returnedSupplierInformation.Should().BeEquivalentTo(supplierInformation);
    }

    [Fact]
    public async Task GetSupplierInformation_OrganisationNotFound_ReturnsNotFound()
    {
        var organisationId = Guid.NewGuid();
        _getSupplierInformationUseCase.Setup(uc => uc.Execute(organisationId)).ReturnsAsync((SupplierInformation?)null);

        var response = await _httpClient.GetAsync($"/organisations/{organisationId}/supplier-information");

        response.StatusCode.Should().Be(NotFound);
    }

    [Fact]
    public async Task GetSupplierInformation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";

        var response = await _httpClient.GetAsync($"/organisations/{invalidOrganisationId}/supplier-information");

        response.StatusCode.Should().Be(UnprocessableEntity);
    }

    [Theory]
    [InlineData(true, NoContent)]
    [InlineData(false, NotFound)]
    public async Task UpdateSupplierInformation_TestCases(bool useCaseResult, HttpStatusCode expectedStatusCode)
    {
        var organisationId = Guid.NewGuid();
        var updateSupplierInformation = new UpdateSupplierInformation { Type = SupplierInformationUpdateType.SupplierType, SupplierInformation = new() };
        var command = (organisationId, updateSupplierInformation);

        if (useCaseResult)
            _updatesSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);
        else
            _updatesSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ThrowsAsync(new UnknownOrganisationException(""));

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{organisationId}/supplier-information", updateSupplierInformation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public async Task UpdateSupplierInformation_InvalidOrganisationId_ReturnsUnprocessableEntity()
    {
        var invalidOrganisationId = "invalid-guid";

        var response = await _httpClient.PatchAsJsonAsync($"/organisations/{invalidOrganisationId}/supplier-information",
            new UpdateSupplierInformation { Type = SupplierInformationUpdateType.SupplierType, SupplierInformation = new() });

        response.StatusCode.Should().Be(UnprocessableEntity);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(OK, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetOrganisationSupplierInformation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();

        _getSupplierInformationUseCase.Setup(uc => uc.Execute(organisationId))
            .ReturnsAsync(new SupplierInformation { OrganisationName = "FakeOrg" });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _getSupplierInformationUseCase.Object));

        var response = await factory.CreateClient().GetAsync($"/organisations/{organisationId}/supplier-information");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Responder)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task UpdateSupplierInformation_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var organisationId = Guid.NewGuid();
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new SupplierInfo()
        };
        var command = (organisationId, updateSupplierInformation);

        _updatesSupplierInformationUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope,
            services => services.AddScoped(_ => _updatesSupplierInformationUseCase.Object));

        var response = await factory.CreateClient().PatchAsJsonAsync($"/organisations/{organisationId}/supplier-information", updateSupplierInformation);

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    private async Task<HttpResponseMessage> SendDeleteRequestAsync(string relativeUri, object obj)
    {
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri(_httpClient.BaseAddress!, relativeUri),
            Content = new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json")
        };
        return await _httpClient.SendAsync(request);
    }
}