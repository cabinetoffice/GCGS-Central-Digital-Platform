using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using System.Net.Http.Json;
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
}