using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Forms.WebApi.Tests.Api;

public class GetFormSectionstEndpointTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<(Guid answerSetId, Guid organisationId), FormSectionResponse?>> _useCase = new();

    public GetFormSectionstEndpointTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services => services.AddScoped(_ => _useCase.Object));
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task GetFormSections_WhenFormIdDoesNotExists_ShouldReturnStatus404()
    {
        _useCase.Setup(uc => uc.Execute(It.IsAny<(Guid, Guid)>())).ReturnsAsync((FormSectionResponse?)null);

        var response = await _httpClient.GetAsync(
            $"/forms/{Guid.NewGuid()}/sections?organisation-id={Guid.NewGuid()}");

        response.StatusCode.Should().Be(NotFound);
    }

    [Fact]
    public async Task GetFormSections_WhenFormIdExists_ShouldReturnOk()
    {
        var formSections = new FormSectionResponse
        {
            FormSections = [new FormSectionSummary
            {
                Type = FormSectionType.Standard,
                AllowsMultipleAnswerSets = true,
                AnswerSetCount = 1,
                SectionId = Guid.NewGuid(),
                SectionName = "TestSection",
                AnswerSetWithFurtherQuestionExemptedExists = false
            }]
        };

        _useCase.Setup(uc => uc.Execute(It.IsAny<(Guid, Guid)>()))
            .ReturnsAsync(formSections);

        var response = await _httpClient.GetFromJsonAsync<FormSectionResponse>(
                $"/forms/{Guid.NewGuid()}/sections?organisation-id={Guid.NewGuid()}");

        response.Should().BeEquivalentTo(formSections);
    }

    [Theory]
    [InlineData(OK, Channel.OneLogin)]
    [InlineData(Forbidden, Channel.ServiceKey)]
    [InlineData(Forbidden, Channel.OrganisationKey)]
    [InlineData(Forbidden, "unknown_channel")]
    public async Task GetFormSections_Authorization_ReturnsExpectedStatusCode(
        HttpStatusCode expectedStatusCode, string channel)
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var command = (formId, organisationId);

        _useCase.Setup(uc => uc.Execute(command))
            .ReturnsAsync(new FormSectionResponse { FormSections = [] });

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, serviceCollection: s => s.AddScoped(_ => _useCase.Object));

        var response = await factory.CreateClient().GetAsync($"/forms/{formId}/sections?organisation-id={organisationId}");

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}