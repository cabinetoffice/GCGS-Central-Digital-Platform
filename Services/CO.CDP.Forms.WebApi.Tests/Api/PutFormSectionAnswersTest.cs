using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net;
using System.Net.Http.Json;
using static CO.CDP.Authentication.Constants;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Forms.WebApi.Tests.Api;

public class PutFormSectionAnswersTest
{
    private readonly Mock<IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, UpdateFormSectionAnswers updateFormSectionAnswers), bool>> _updateFormSectionAnswersUseCase = new();

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
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var updateFormSectionAnswers = new UpdateFormSectionAnswers();
        var command = (formId, sectionId, answerSetId, organisationId, updateFormSectionAnswers);

        _updateFormSectionAnswersUseCase.Setup(uc => uc.Execute(command)).ReturnsAsync(true);

        var factory = new TestAuthorizationWebApplicationFactory<Program>(
            channel, organisationId, scope, serviceCollection: s => s.AddScoped(_ => _updateFormSectionAnswersUseCase.Object));

        var response = await factory.CreateClient().PutAsJsonAsync(
            $"/forms/{formId}/sections/{sectionId}/answers/{answerSetId}?organisation-id={organisationId}", updateFormSectionAnswers);

        response.StatusCode.Should().Be(expectedStatusCode);
    }
}
