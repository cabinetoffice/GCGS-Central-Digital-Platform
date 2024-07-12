using FluentAssertions;
using Moq;
using WebApiClient = CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests;

public class FormsEngineTests
{
    private readonly Mock<WebApiClient.IFormsClient> _formsApiClientMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly FormsEngine _formsEngine;

    public FormsEngineTests()
    {
        _formsApiClientMock = new Mock<WebApiClient.IFormsClient>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _formsEngine = new FormsEngine(_formsApiClientMock.Object, _tempDataServiceMock.Object);
    }

    [Fact]
    public async Task LoadFormSectionAsync_ShouldReturnCachedResponse_WhenCachedResponseExists()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var sessionKey = $"SectionQuestionsResponse_{formId}_{sectionId}";

        var cachedResponse = new Models.SectionQuestionsResponse
        {
            Section = new Models.FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<Models.FormQuestion>()
        };

        _tempDataServiceMock
            .Setup(t => t.Get<Models.SectionQuestionsResponse>(sessionKey))
            .Returns(cachedResponse);

        var result = await _formsEngine.LoadFormSectionAsync(formId, sectionId);

        result.Should().BeEquivalentTo(cachedResponse);
        _formsApiClientMock.Verify(c => c.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

}