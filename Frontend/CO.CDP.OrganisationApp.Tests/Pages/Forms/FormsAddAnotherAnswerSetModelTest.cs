using WebApiClient = CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsAddAnotherAnswerSetModelTest
{
    private Mock<WebApiClient.IFormsClient> _formsClientMock;
    private Mock<ITempDataService> _tempDataServiceMock;
    private FormsAddAnotherAnswerSetModel _model;

    public FormsAddAnotherAnswerSetModelTest()
    {
        _formsClientMock = new Mock<WebApiClient.IFormsClient>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _model = new FormsAddAnotherAnswerSetModel(_formsClientMock.Object, _tempDataServiceMock.Object)
        {
            OrganisationId = Guid.NewGuid(),
            FormId = Guid.NewGuid(),
            SectionId = Guid.NewGuid()
        };
    }

    private static WebApiClient.SectionQuestionsResponse CreateApiSectionQuestionsResponse(Guid sectionId, Guid questionId, Guid nextQuestionId)
    {
        return new WebApiClient.SectionQuestionsResponse(
            section: new WebApiClient.FormSection(
                allowsMultipleAnswerSets: true,
                configuration: new WebApiClient.FormSectionConfiguration(
                    singularSummaryHeading: null,
                    pluralSummaryHeadingFormat: null,
                    addAnotherAnswerLabel: null,
                    removeConfirmationCaption: null,
                    removeConfirmationHeading: null
                ),
                id: sectionId,
                title: "SectionTitle"
            ),
            questions: new List<WebApiClient.FormQuestion>
            {
            new WebApiClient.FormQuestion(
                id: questionId,
                title: "Question1",
                description: "Description1",
                type: WebApiClient.FormQuestionType.Text,
                isRequired: true,
                nextQuestion: nextQuestionId,
                nextQuestionAlternative: null,
                options: new WebApiClient.FormQuestionOptions(
                    choiceProviderStrategy: null,
                    choices: new List<WebApiClient.FormQuestionChoice>
                    {
                        new WebApiClient.FormQuestionChoice(
                            id: Guid.NewGuid(),
                            title: "Option1",
                            groupName: null,
                            hint: new WebApiClient.FormQuestionChoiceHint(
                                title: null,
                                description: "Hint Description"
                            )
                        )
                    }
                )
            )
            },
            answerSets: new List<WebApiClient.FormAnswerSet>()
        );
    }

    [Fact]
    public async Task OnGet_ShouldReturnPage_WhenInitAndVerifyPageReturnsTrue()
    {
        // Arrange
        var sectionQuestionsResponse = CreateApiSectionQuestionsResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(sectionQuestionsResponse);

        // Act
        var result = await _model.OnGet();

        // Assert
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToPageNotFound_WhenInitAndVerifyPageReturnsFalse()
    {
        // Arrange
        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ThrowsAsync(new WebApiClient.ApiException("Unexpected error", 404, "", default, null));

        // Act
        var result = await _model.OnGet();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToDynamicFormsPage_WhenAddAnotherAnswerSetIsTrue()
    {
        // Arrange
        _model.AddAnotherAnswerSet = true;

        var sectionQuestionsResponse = CreateApiSectionQuestionsResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(sectionQuestionsResponse);

        // Act
        var result = await _model.OnPost();

        // Assert
        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("DynamicFormsPage");
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToSupplierBasicInformation_WhenAddAnotherAnswerSetIsFalse()
    {
        // Arrange
        _model.AddAnotherAnswerSet = false;

        var sectionQuestionsResponse = CreateApiSectionQuestionsResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(sectionQuestionsResponse);

        // Act
        var result = await _model.OnPost();

        // Assert
        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("SupplierBasicInformation");
    }

    [Fact]
    public async Task OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        // Arrange
        _model.ModelState.AddModelError("key", "error message");

        var sectionQuestionsResponse = CreateApiSectionQuestionsResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(sectionQuestionsResponse);

        // Act
        var result = await _model.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnGetChange_ShouldRedirectToDynamicFormsPage_WhenAnswerSetIsFound()
    {
        // Arrange
        var answerSetId = Guid.NewGuid();
        var sectionQuestionsResponse = CreateApiSectionQuestionsResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(sectionQuestionsResponse);

        // Act
        var result = await _model.OnGetChange(answerSetId);

        // Assert
        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectToPageResult.PageName.Should().Be("DynamicFormsPage");
    }

    [Fact]
    public async Task OnGetChange_ShouldRedirectToPageNotFound_WhenAnswerSetIsNotFound()
    {
        // Arrange
        var answerSetId = Guid.NewGuid();
        var sectionQuestionsResponse = CreateApiSectionQuestionsResponse(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        _formsClientMock
            .Setup(client => client.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(sectionQuestionsResponse);

        // Act
        var result = await _model.OnGetChange(answerSetId);

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }
}