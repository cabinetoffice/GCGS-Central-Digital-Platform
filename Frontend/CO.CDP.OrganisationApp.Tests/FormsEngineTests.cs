using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms.ChoiceProviderStrategies;
using FluentAssertions;
using Moq;
using DataShareWebApiClient = CO.CDP.DataSharing.WebApiClient;
using WebApiClient = CO.CDP.Forms.WebApiClient;

namespace CO.CDP.OrganisationApp.Tests;

public class FormsEngineTests
{
    private readonly Mock<WebApiClient.IFormsClient> _formsApiClientMock;
    private readonly Mock<DataShareWebApiClient.IDataSharingClient> _dataSharingClientMock;
    private readonly Mock<ITempDataService> _tempDataServiceMock;
    private readonly Mock<IChoiceProviderService> _choiceProviderServiceMock;
    private readonly Mock<IUserInfoService> _userInfoServiceMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly FormsEngine _formsEngine;

    public FormsEngineTests()
    {
        _formsApiClientMock = new Mock<WebApiClient.IFormsClient>();
        _dataSharingClientMock = new Mock<DataShareWebApiClient.IDataSharingClient>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        _userInfoServiceMock = new Mock<IUserInfoService>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _formsEngine = new FormsEngine(_formsApiClientMock.Object, _tempDataServiceMock.Object, _choiceProviderServiceMock.Object, _dataSharingClientMock.Object);
    }

    private static (Guid organisationId, Guid formId, Guid sectionId, string sessionKey) CreateTestGuids()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        return (organisationId, formId, sectionId, sessionKey);
    }

    private static WebApiClient.SectionQuestionsResponse CreateApiSectionQuestionsResponse(Guid sectionId, Guid questionId, Guid nextQuestionId, string? choiceProviderStrategy = null, string? answerFieldName = null)
    {
        return new WebApiClient.SectionQuestionsResponse(
            section: new WebApiClient.FormSection(
                type: WebApiClient.FormSectionType.Standard,
                allowsMultipleAnswerSets: true,
                checkFurtherQuestionsExempted: false,
                configuration: new WebApiClient.FormSectionConfiguration(
                    addAnotherAnswerLabel: null,
                    furtherQuestionsExemptedHeading: null,
                    furtherQuestionsExemptedHint: null,
                    pluralSummaryHeadingFormat: null,
                    pluralSummaryHeadingHintFormat: null,
                    removeConfirmationCaption: null,
                    removeConfirmationHeading: null,
                    singularSummaryHeading: null,
                    singularSummaryHeadingHint: null,
                    summaryRenderFormatter: null
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
                        caption: "Caption1",
                        summaryTitle: "Question1 Title",
                        type: WebApiClient.FormQuestionType.Text,
                        isRequired: true,
                        nextQuestion: nextQuestionId,
                        nextQuestionAlternative: null,
                        name: "Question1",
                        options: new WebApiClient.FormQuestionOptions(
                            choiceProviderStrategy: choiceProviderStrategy,
                            answerFieldName: answerFieldName,
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
                            },
                            groups: new List<WebApiClient.FormQuestionGroup>
                            {
                                new WebApiClient.FormQuestionGroup(
                                    name: "Group 1",
                                    hint: "Group 1 Hint",
                                    caption: "Group 1 Caption",
                                    choices: new List<WebApiClient.FormQuestionGroupChoice>
                                    {
                                        new WebApiClient.FormQuestionGroupChoice(
                                            title: "Group Choice 1",
                                            value: "group_choice_1"
                                        ),
                                        new WebApiClient.FormQuestionGroupChoice(
                                            title: "Group Choice 2",
                                            value: "group_choice_2"
                                        )
                                    }
                                )
                            }
                        )
                    )
                },
                answerSets: new List<WebApiClient.FormAnswerSet>()
            );
    }

    private static SectionQuestionsResponse CreateModelSectionQuestionsResponse(Guid sectionId, Guid questionId, Guid nextQuestionId, string? choiceProviderStrategy = null, Dictionary<string, string>? options = null)
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
                Caption= "Caption1",
                SummaryTitle= "Question1 Title",
                Type = FormQuestionType.Text,
                IsRequired = true,
                NextQuestion = nextQuestionId,
                Options = new FormQuestionOptions
                {
                    Choices = options == null ? new Dictionary<string, string>() { { "Option1", "Option1" } } : options,
                    ChoiceProviderStrategy = choiceProviderStrategy
                }
            }
        }
        };
    }

    [Fact]
    public async Task GetFormSectionAsync_ShouldReturnCachedResponse_WhenCachedResponseExists()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var cachedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId);

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns(cachedResponse);

        var result = await _formsEngine.GetFormSectionAsync(organisationId, formId, sectionId);

        result.Should().BeEquivalentTo(cachedResponse);
        _formsApiClientMock.Verify(c => c.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetFormSectionAsync_ShouldFetchAndCacheResponse_WhenCachedResponseDoesNotExist()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();

        var apiResponse = CreateApiSectionQuestionsResponse(sectionId, questionId, nextQuestionId);

        var expectedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId);

        expectedResponse.Questions[0].Options.Groups = new List<FormQuestionGroup>
        {
            new FormQuestionGroup
            {
                Name = "Group 1",
                Hint = "Group 1 Hint",
                Caption = "Group 1 Caption",
                Choices = new List<FormQuestionGroupChoice>
                {
                    new FormQuestionGroupChoice { Title = "Group Choice 1", Value = "group_choice_1" },
                    new FormQuestionGroupChoice { Title = "Group Choice 2", Value = "group_choice_2" }
                }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns((SectionQuestionsResponse?)null);
        _choiceProviderServiceMock.Setup(t => t.GetStrategy(It.IsAny<string>()))
            .Returns(new DefaultChoiceProviderStrategy());
        _formsApiClientMock.Setup(c => c.GetFormSectionQuestionsAsync(formId, sectionId, organisationId))
            .ReturnsAsync(apiResponse);

        var result = await _formsEngine.GetFormSectionAsync(organisationId, formId, sectionId);

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

        var result = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, null);

        result.Should().BeEquivalentTo(sectionResponse.Questions.First(q => q.Id == nextQuestionId));
    }

    [Fact]
    public async Task GetNextQuestion_ShouldReturnNextQuestionAlternative_WhenAnswerIsNoForYesNoQuestion()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = currentQuestionId, NextQuestion = nextQuestionId, NextQuestionAlternative = nextQuestionAlternativeId, Type = FormQuestionType.YesOrNo },
                new FormQuestion { Id = nextQuestionId },
                new FormQuestion { Id = nextQuestionAlternativeId }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = currentQuestionId, Answer = new FormAnswer { BoolValue = false } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(nextQuestionAlternativeId);
    }

    [Fact]
    public async Task GetNextQuestion_ShouldReturnNextQuestion_WhenAnswerIsTrueForYesNoQuestion()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion { Id = currentQuestionId, NextQuestion = nextQuestionId, NextQuestionAlternative = nextQuestionAlternativeId, Type = FormQuestionType.YesOrNo },
                new FormQuestion { Id = nextQuestionId },
                new FormQuestion { Id = nextQuestionAlternativeId }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = currentQuestionId, Answer = new FormAnswer { BoolValue = true } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result = await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(nextQuestionId);
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

    [Fact]
    public async Task GetFormSectionAsync_ShouldFetchChoicesFromCustomChoiceProvider_WhenCustomChoiceProviderIsConfigured()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var apiResponse = CreateApiSectionQuestionsResponse(sectionId, questionId, nextQuestionId, "ExclusionAppliesToChoiceProviderStrategy", "JsonValue");
        var expectedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId, "ExclusionAppliesToChoiceProviderStrategy");

        expectedResponse.Questions[0].Options.Choices = new Dictionary<string, string>() {
            { $@"{{""id"":""{organisationId}"",""type"":""organisation""}}", "User's current organisation" },
            { "{\"id\":\"e4bdd7ef-8200-4257-9892-b16f43d1803e\",\"type\":\"connected-entity\"}", "First name Last name" },
            { "{\"id\":\"4c8dccba-df39-4997-814b-7599ed9b5bed\",\"type\":\"connected-entity\"}", "Connected organisation" } };

        expectedResponse.Questions[0].Options.AnswerFieldName = "JsonValue";

        expectedResponse.Questions[0].Options.Groups = new List<FormQuestionGroup>
        {
            new FormQuestionGroup
            {
                Name = "Group 1",
                Hint = "Group 1 Hint",
                Caption = "Group 1 Caption",
                Choices = new List<FormQuestionGroupChoice>
                {
                    new FormQuestionGroupChoice { Title = "Group Choice 1", Value = "group_choice_1" },
                    new FormQuestionGroupChoice { Title = "Group Choice 2", Value = "group_choice_2" }
                }
            }
        };

        var connectedIndividualGuid = new Guid("e4bdd7ef-8200-4257-9892-b16f43d1803e");

        _organisationClientMock.Setup(c => c.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync([
                new ConnectedEntityLookup(endDate: null, entityId: connectedIndividualGuid, entityType: ConnectedEntityType.Individual, name: "Connected person", uri: new Uri("http://whatever"), deleted: false, isInUse: false, formGuid: null, sectionGuid: null),
                new ConnectedEntityLookup(endDate: null, entityId: new Guid("4c8dccba-df39-4997-814b-7599ed9b5bed"), entityType: ConnectedEntityType.Organisation, name: "Connected organisation", uri: new Uri("http://whatever"), deleted: false, isInUse: false, formGuid: null, sectionGuid: null)
            ]);
        _organisationClientMock.Setup(c => c.GetConnectedEntityAsync(organisationId, connectedIndividualGuid))
            .ReturnsAsync(new ConnectedEntity(
                            [],
                            "123",
                            null,
                            ConnectedEntityType.Individual,
                            false,
                            connectedIndividualGuid,
                            new ConnectedIndividualTrust(
                                ConnectedIndividualAndTrustCategory.PersonWithSignificantControlForIndividual,
                                ConnectedPersonType.Individual,
                                [],
                                null,
                                "First name",
                                3,
                                "Last name",
                                "British",
                                new Guid(),
                                "UK"
                            ),
                            new ConnectedOrganisation(
                                ConnectedOrganisationCategory.AnyOtherOrganisationWithSignificantInfluenceOrControl,
                                [],
                                4,
                                null,
                                "law",
                                "name",
                                organisationId,
                                "legal form"
                            ),
                            "123",
                            null,
                            "register name"
                        ));
        _organisationClientMock.Setup(c => c.GetOrganisationAsync(organisationId))
            .ReturnsAsync(new Organisation.WebApiClient.Organisation(additionalIdentifiers: [], addresses: [], contactPoint: null, id: organisationId, identifier: null, name: "User's current organisation", type: OrganisationType.Organisation, roles: [], details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)));
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(organisationId);
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns((SectionQuestionsResponse?)null);
        _choiceProviderServiceMock.Setup(t => t.GetStrategy("ExclusionAppliesToChoiceProviderStrategy"))
            .Returns(new ExclusionAppliesToChoiceProviderStrategy(_userInfoServiceMock.Object, _organisationClientMock.Object));
        _formsApiClientMock.Setup(c => c.GetFormSectionQuestionsAsync(formId, sectionId, organisationId))
            .ReturnsAsync(apiResponse);

        var result = await _formsEngine.GetFormSectionAsync(organisationId, formId, sectionId);

        result.Should().BeEquivalentTo(expectedResponse);
        _tempDataServiceMock.Verify(t => t.Put(sessionKey, It.IsAny<SectionQuestionsResponse>()), Times.Once);
    }

    [Fact]
    public async Task SaveUpdateAnswers_ShouldCallApiWithCorrectPayload_WhenAnswerSetIdIsNull()
    {
        var (formId, sectionId, organisationId, answerSet, expectedAnswer) = SetupTestData();
        await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);

        _formsApiClientMock.Verify(api => api.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            It.IsAny<Guid>(),
            organisationId,
            It.Is<WebApiClient.UpdateFormSectionAnswers>(payload =>
                payload.Answers.Count == 1 &&
                payload.Answers.Any(a =>
                    (!expectedAnswer.BoolValue.HasValue || a.BoolValue == expectedAnswer.BoolValue.Value) &&
                    (!expectedAnswer.NumericValue.HasValue || a.NumericValue == expectedAnswer.NumericValue.Value) &&
                    (!expectedAnswer.DateValue.HasValue || a.DateValue == expectedAnswer.DateValue.Value) &&
                    (expectedAnswer.TextValue == null || a.TextValue == expectedAnswer.TextValue) &&
                    (expectedAnswer.OptionValue == null || a.OptionValue == expectedAnswer.OptionValue)
                )
            )
        ), Times.Once);
    }

    [Fact]
    public async Task SaveUpdateAnswers_ShouldCallApiWithCorrectPayload_WhenAnswerSetIdIsProvided()
    {
        var (formId, sectionId, organisationId, answerSet, expectedAnswer) = SetupTestData();
        var existingAnswerSetId = Guid.NewGuid();
        answerSet.AnswerSetId = existingAnswerSetId;
        await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);

        _formsApiClientMock.Verify(api => api.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            existingAnswerSetId,
            organisationId,
            It.Is<WebApiClient.UpdateFormSectionAnswers>(payload =>
                payload.Answers.Count == 1 &&
                payload.Answers.Any(a =>
                    (!expectedAnswer.BoolValue.HasValue || a.BoolValue == expectedAnswer.BoolValue.Value) &&
                    (!expectedAnswer.NumericValue.HasValue || a.NumericValue == expectedAnswer.NumericValue.Value) &&
                    (!expectedAnswer.DateValue.HasValue || a.DateValue == expectedAnswer.DateValue.Value) &&
                    (expectedAnswer.TextValue == null || a.TextValue == expectedAnswer.TextValue) &&
                    (expectedAnswer.OptionValue == null || a.OptionValue == expectedAnswer.OptionValue)
                )
            )
        ), Times.Once);
    }

    [Fact]
    public async Task SaveUpdateAnswers_ShouldThrowException_WhenApiCallFails()
    {
        var (formId, sectionId, organisationId, answerSet, _) = SetupTestData();

        _formsApiClientMock.Setup(api => api.PutFormSectionAnswersAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<WebApiClient.UpdateFormSectionAnswers>()))
            .ThrowsAsync(new Exception("API call failed"));

        Func<Task> act = async () => await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);
        await act.Should().ThrowAsync<Exception>().WithMessage("API call failed");
    }

    [Fact]
    public async Task SaveUpdateAnswers_ShouldCallApiWithCorrectPayload_WhenYesNoChangesAndAlternativeAnswersAreCleared()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var yesNoQuestionId = Guid.NewGuid();
        var alternativeQuestionId = Guid.NewGuid();
        var mainPathQuestionId = Guid.NewGuid();

        var answerSet = new FormQuestionAnswerState
        {
            AnswerSetId = Guid.NewGuid(),
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = yesNoQuestionId,
                    Answer = new FormAnswer { BoolValue = true }
                },
                new QuestionAnswer
                {
                    QuestionId = mainPathQuestionId,
                    Answer = new FormAnswer { TextValue = "Main path answer" }
                }
            }
        };

        await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);

        _formsApiClientMock.Verify(api => api.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            answerSet.AnswerSetId.Value,
            organisationId,
            It.Is<WebApiClient.UpdateFormSectionAnswers>(payload =>
                payload.Answers.Count == 2 &&
                payload.Answers.Any(a => a.QuestionId == yesNoQuestionId && a.BoolValue == true) &&
                payload.Answers.Any(a => a.QuestionId == mainPathQuestionId && a.TextValue == "Main path answer") &&
                !payload.Answers.Any(a => a.QuestionId == alternativeQuestionId)
            )
        ), Times.Once);
    }

    [Fact]
    public async Task SaveUpdateAnswers_ShouldCallApiWithCorrectPayload_WhenFileUploadChangesAndAlternativeAnswersAreCleared()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        var fileUploadQuestionId = Guid.NewGuid();
        var alternativeQuestionId = Guid.NewGuid();
        var mainPathQuestionId = Guid.NewGuid();

        var answerSet = new FormQuestionAnswerState
        {
            AnswerSetId = Guid.NewGuid(),
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = fileUploadQuestionId,
                    Answer = new FormAnswer { BoolValue = true, TextValue = "uploaded_file.pdf" }
                },
                new QuestionAnswer
                {
                    QuestionId = mainPathQuestionId,
                    Answer = new FormAnswer { TextValue = "Main path answer after file upload" }
                }
            }
        };

        await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);

        _formsApiClientMock.Verify(api => api.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            answerSet.AnswerSetId.Value,
            organisationId,
            It.Is<WebApiClient.UpdateFormSectionAnswers>(payload =>
                payload.Answers.Count == 2 &&
                payload.Answers.Any(a => a.QuestionId == fileUploadQuestionId && a.BoolValue == true && a.TextValue == "uploaded_file.pdf") &&
                payload.Answers.Any(a => a.QuestionId == mainPathQuestionId && a.TextValue == "Main path answer after file upload") &&
                !payload.Answers.Any(a => a.QuestionId == alternativeQuestionId)
            )
        ), Times.Once);
    }

    [Fact]
    public async Task CreateShareCodeAsync_ShouldReturnShareCode_WhenApiCallSucceeds()
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var expectedShareCode = "HDJ2123F";

        _dataSharingClientMock.Setup(client => client.CreateSharedDataAsync(
            It.Is<DataShareWebApiClient.ShareRequest>(sr =>
                sr.FormId == formId && sr.OrganisationId == organisationId)))
            .ReturnsAsync(new DataShareWebApiClient.ShareReceipt(formId, null, expectedShareCode));

        var result = await _formsEngine.CreateShareCodeAsync(formId, organisationId);

        result.Should().Be(expectedShareCode);
        _dataSharingClientMock.Verify(client => client.CreateSharedDataAsync(
            It.Is<DataShareWebApiClient.ShareRequest>(sr =>
                sr.FormId == formId && sr.OrganisationId == organisationId)), Times.Once);
    }

    [Fact]
    public async Task CreateShareCodeAsync_ShouldThrowException_WhenApiCallFails()
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _dataSharingClientMock.Setup(client => client.CreateSharedDataAsync(
            It.IsAny<DataShareWebApiClient.ShareRequest>()))
            .ThrowsAsync(new Exception("API call failed"));

        Func<Task> act = async () => await _formsEngine.CreateShareCodeAsync(formId, organisationId);

        await act.Should().ThrowAsync<Exception>().WithMessage("API call failed");
        _dataSharingClientMock.Verify(client => client.CreateSharedDataAsync(
            It.Is<DataShareWebApiClient.ShareRequest>(sr =>
                sr.FormId == formId && sr.OrganisationId == organisationId)), Times.Once);
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnNull_WhenAllQuestionsAreAnswered()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid() };
        var question2 = new FormQuestion { Id = Guid.NewGuid() };
        var question3 = new FormQuestion { Id = Guid.NewGuid() };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
        {
            new QuestionAnswer { QuestionId = question1.Id },
            new QuestionAnswer { QuestionId = question2.Id },
            new QuestionAnswer { QuestionId = question3.Id }
        }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().BeNull("because all questions have been answered");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnFirstQuestion_WhenNoQuestionsAreAnswered()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid() };
        var question2 = new FormQuestion { Id = Guid.NewGuid() };
        var question3 = new FormQuestion { Id = Guid.NewGuid() };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>()
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().Be(question1.Id, "because no questions have been answered, so the first question should be returned");
    }


    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnExpectedResult_WhenCalled()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid() };
        var question2 = new FormQuestion { Id = Guid.NewGuid() };
        var question3 = new FormQuestion { Id = Guid.NewGuid() };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
        {
            new QuestionAnswer { QuestionId = question2.Id }
        }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().Be(question1.Id);
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnNull_WhenAllPreviousQuestionsAreAnswered()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid() };
        var question2 = new FormQuestion { Id = Guid.NewGuid() };
        var question3 = new FormQuestion { Id = Guid.NewGuid() };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
        {
            new QuestionAnswer { QuestionId = question1.Id },
            new QuestionAnswer { QuestionId = question2.Id }
        }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().BeNull();
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldNotStartSortedListWithBranchedQuestion_WhenCalled()
    {
        var qActualStart = new FormQuestion { Id = Guid.NewGuid(), Title = "Actual Start" };
        var qAlternativeOnlyTarget = new FormQuestion { Id = Guid.NewGuid(), Title = "Alternative Only Target" };
        var qIntermediate = new FormQuestion { Id = Guid.NewGuid(), Title = "Intermediate" };
        var qEnd = new FormQuestion { Id = Guid.NewGuid(), Title = "End" };

        qActualStart.NextQuestion = qIntermediate.Id;
        qIntermediate.NextQuestion = qEnd.Id;
        qIntermediate.NextQuestionAlternative = qAlternativeOnlyTarget.Id;

        var questions = new List<FormQuestion> { qAlternativeOnlyTarget, qActualStart, qIntermediate, qEnd };
        var answerState = new FormQuestionAnswerState { Answers = new List<QuestionAnswer>() };
        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, qIntermediate.Id, answerState);

        result.Should().Be(qActualStart.Id);
    }

    private (Guid formId, Guid sectionId, Guid organisationId, FormQuestionAnswerState answerSet, FormAnswer expectedAnswer) SetupTestData()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var answerSet = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
                {
                    new QuestionAnswer
                    {
                        QuestionId = Guid.NewGuid(),
                        Answer = new FormAnswer
                        {
                            BoolValue = true,
                            NumericValue = 42,
                            DateValue = DateTimeOffset.UtcNow,
                            TextValue = "Sample Answer",
                            OptionValue = "Option1"
                        }
                    }
                }
        };

        var expectedAnswer = answerSet.Answers[0].Answer;

        if (expectedAnswer == null)
        {
            throw new InvalidOperationException("Expected answer should not be null for this test case.");
        }

        return (formId, sectionId, organisationId, answerSet, expectedAnswer);
    }
}