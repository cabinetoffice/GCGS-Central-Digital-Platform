using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Xunit.Abstractions;

namespace CO.CDP.Forms.WebApiClient.Tests;

public class FormsClientIntegrationTest(ITestOutputHelper testOutputHelper)
{
    private readonly TestWebApplicationFactory<Program> _factory = new(builder =>
    {
        builder.ConfigureLogging(testOutputHelper);
    });

    [Fact]
    public async Task ItTalksToTheFormsApi()
    {
        IFormsClient client = new FormsClient("https://localhost", _factory.CreateClient());
        var formId = Guid.Parse("a38b44a4-7a13-409d-912e-b88b9958e54a");
        var sectionId = Guid.Parse("3b7a1483-199c-431f-a230-e8a5e572dec4");

        var formSection = await client.GetFormSectionQuestionsAsync(formId, sectionId);

        formSection.Should().NotBeNull();
    }
}