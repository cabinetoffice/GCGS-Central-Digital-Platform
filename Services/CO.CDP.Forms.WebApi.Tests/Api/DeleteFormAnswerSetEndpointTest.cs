using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Forms.WebApi.Tests.Api;

public class DeleteFormAnswerSetEndpointTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<(Guid answerSetId, Guid organisationId), bool>> _deleteAnswerSetUseCase = new();

    public DeleteFormAnswerSetEndpointTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped(_ => _deleteAnswerSetUseCase.Object)
            );
        });
        _httpClient = factory.CreateClient();
    }

    [Theory]
    [InlineData(true, NoContent)]
    [InlineData(false, NotFound)]
    public async Task DeleteSupplierInformation_TestCases(bool useCaseResult, HttpStatusCode expectedStatusCode)
    {
        _deleteAnswerSetUseCase.Setup(uc => uc.Execute(It.IsAny<(Guid, Guid)>())).ReturnsAsync(useCaseResult);

        var response = await _httpClient.DeleteAsync($"/forms/answer-sets/{Guid.NewGuid()}?organisation-id={Guid.NewGuid()}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }

    [Theory]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Admin)]
    [InlineData(NoContent, Channel.OneLogin, OrganisationPersonScope.Editor)]
    [InlineData(Forbidden, Channel.OneLogin, OrganisationPersonScope.Viewer)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetFormSectionQuestions_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel, string? scope = null)
    {
        var answerSetId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var command = (organisationId, answerSetId);

        _deleteAnswerSetUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope, serviceCollection: s => s.AddScoped(_ => _deleteAnswerSetUseCase.Object));

        var response = await factory.CreateClient().DeleteAsync($"/forms/answer-sets/{answerSetId}?organisation-id={organisationId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}