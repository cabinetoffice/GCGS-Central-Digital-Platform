using CO.CDP.OrganisationApp.Models;
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

    private static (Guid organisationId, Guid formId, Guid sectionId, string sessionKey) CreateTestGuids()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        return (organisationId, formId, sectionId, sessionKey);
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

    private static SectionQuestionsResponse CreateModelSectionQuestionsResponse(Guid sectionId, Guid questionId, Guid nextQuestionId)
    {
        return new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = questionId,
                Title = "Question1",
                Description = "Description1",
                Type = FormQuestionType.Text,
                IsRequired = true,
                NextQuestion = nextQuestionId,
                Options = new FormQuestionOptions
                {
                    Choices = new List<string> { "Option1" }
                }
            }
        }
        };
    }

    [Fact]
    public async Task LoadFormSectionAsync_ShouldReturnCachedResponse_WhenCachedResponseExists()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var cachedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId);

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(cachedResponse);

        var result = await _formsEngine.LoadFormSectionAsync(organisationId, formId, sectionId);

        result.Should().BeEquivalentTo(cachedResponse);
        _formsApiClientMock.Verify(c => c.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task LoadFormSectionAsync_ShouldFetchAndCacheResponse_WhenCachedResponseDoesNotExist()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();        
        var apiResponse = CreateApiSectionQuestionsResponse(sectionId, questionId, nextQuestionId);
        var expectedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId);

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns((SectionQuestionsResponse?)null);
        _formsApiClientMock.Setup(c => c.GetFormSectionQuestionsAsync(formId, sectionId,organisationId))
            .ReturnsAsync(apiResponse);

        var result = await _formsEngine.LoadFormSectionAsync(organisationId, formId, sectionId);

        result.Should().BeEquivalentTo(expectedResponse);
        _tempDataServiceMock.Verify(t => t.Put(sessionKey, It.IsAny<SectionQuestionsResponse>()), Times.Once);
    }

    [Fact]
    public async Task GetNextQuestion_ShouldReturnNextQuestion_WhenCurrentQuestionExists()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = currentQuestionId, NextQuestion = nextQuestionId },
                new FormQuestion { Id = nextQuestionId }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId);

        result.Should().BeEquivalentTo(sectionResponse.Questions.First(q => q.Id == nextQuestionId));
    }

    [Fact]
    public async Task GetPreviousQuestion_ShouldReturnPreviousQuestion_WhenCurrentQuestionExists()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var previousQuestionId = Guid.NewGuid();
        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = previousQuestionId, NextQuestion = currentQuestionId },
                new FormQuestion { Id = currentQuestionId }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetPreviousQuestion(organisationId, formId, sectionId, currentQuestionId);

        result.Should().BeEquivalentTo(sectionResponse.Questions.First(q => q.Id == previousQuestionId));
    }

    [Fact]
    public async Task GetCurrentQuestion_ShouldReturnFirstQuestion_WhenQuestionIdIsNull()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var firstQuestionId = Guid.NewGuid();
        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = firstQuestionId }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetCurrentQuestion(organisationId, formId, sectionId, null);

        result.Should().BeEquivalentTo(sectionResponse.Questions.First(q => q.Id == firstQuestionId));
    }

    [Fact]
    public async Task GetCurrentQuestion_ShouldReturnSpecifiedQuestion_WhenQuestionIdIsNotNull()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = questionId }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetCurrentQuestion(organisationId, formId, sectionId, questionId);

        result.Should().BeEquivalentTo(sectionResponse.Questions.First(q => q.Id == questionId));
    }
}