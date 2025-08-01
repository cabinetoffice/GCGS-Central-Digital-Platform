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
    private const double Tolerance = 1e-6;
    private static readonly Guid GroupId = Guid.NewGuid();

    public FormsEngineTests()
    {
        _formsApiClientMock = new Mock<WebApiClient.IFormsClient>();
        _dataSharingClientMock = new Mock<DataShareWebApiClient.IDataSharingClient>();
        _tempDataServiceMock = new Mock<ITempDataService>();
        _choiceProviderServiceMock = new Mock<IChoiceProviderService>();
        _userInfoServiceMock = new Mock<IUserInfoService>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _formsEngine = new FormsEngine(_formsApiClientMock.Object, _tempDataServiceMock.Object,
            _choiceProviderServiceMock.Object, _dataSharingClientMock.Object);
    }

    private static (Guid organisationId, Guid formId, Guid sectionId, string sessionKey) CreateTestGuids()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var sessionKey = $"Form_{organisationId}_{formId}_{sectionId}_Questions";
        return (organisationId, formId, sectionId, sessionKey);
    }

    private static WebApiClient.SectionQuestionsResponse CreateApiSectionQuestionsResponse(Guid sectionId,
        Guid questionId, Guid nextQuestionId, string? choiceProviderStrategy = null, string? answerFieldName = null,
        Guid? nextQuestionAlternative = null)
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
                    nextQuestionAlternative: nextQuestionAlternative,
                    name: "Question1",
                    options: new WebApiClient.FormQuestionOptions(
                        choiceProviderStrategy: choiceProviderStrategy,
                        answerFieldName: answerFieldName,
                        choices: new List<WebApiClient.FormQuestionChoice>
                        {
                            new WebApiClient.FormQuestionChoice(
                                id: GroupId,
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
                        },
                        grouping: new WebApiClient.FormQuestionGrouping(
                            id: GroupId,
                            page: true,
                            checkYourAnswers: true,
                            summaryTitle: "SummaryTitle"
                        ),
                        layout: new WebApiClient.LayoutOptions(
                            customYesText: null,
                            customNoText: null,
                            inputWidth: null,
                            inputSuffix: null,
                            customCssClasses: null,
                            beforeTitleContent: null,
                            beforeButtonContent: null,
                            afterButtonContent: null,
                            primaryButtonText: null,
                            headingSize: null
                        ),
                        validation: new WebApiClient.ValidationOptions(
                            dateValidationType: null,
                            textValidationType: null,
                            minDate: null,
                            maxDate: null
                        )
                    )
                )
            },
            answerSets: new List<WebApiClient.FormAnswerSet>()
        );
    }

    private static SectionQuestionsResponse CreateModelSectionQuestionsResponse(Guid sectionId, Guid questionId,
        Guid nextQuestionId, string? choiceProviderStrategy = null, Dictionary<string, string>? options = null)
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
                    Caption = "Caption1",
                    SummaryTitle = "Question1 Title",
                    Type = FormQuestionType.Text,
                    IsRequired = true,
                    NextQuestion = nextQuestionId,
                    Options = new FormQuestionOptions
                    {
                        Choices = options == null
                            ? new Dictionary<string, string>() { { "Option1", "Option1" } }
                            : options,
                        ChoiceProviderStrategy = choiceProviderStrategy,
                        Layout = new LayoutOptions
                        {
                            CustomYesText = null,
                            CustomNoText = null,
                            InputWidth = null,
                            InputSuffix = null,
                            CustomCssClasses = null,
                            BeforeTitleContent = null,
                            BeforeButtonContent = null,
                            AfterButtonContent = null,
                            PrimaryButtonText = null
                        },
                        Validation = new ValidationOptions
                        {
                            DateValidationType = null,
                            MinDate = null,
                            MaxDate = null,
                            TextValidationType = null
                        }
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
        _formsApiClientMock.Verify(
            c => c.GetFormSectionQuestionsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetFormSectionAsync_ShouldFetchAndCacheResponse_WhenCachedResponseDoesNotExist()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var nextQuestionAlternativeId = Guid.NewGuid();

        var apiResponse = CreateApiSectionQuestionsResponse(sectionId, questionId, nextQuestionId,
            nextQuestionAlternative: nextQuestionAlternativeId);

        var expectedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId);
        expectedResponse.Questions[0].NextQuestionAlternative = nextQuestionAlternativeId;

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

        expectedResponse.Questions[0].Options.Grouping = new FormQuestionGrouping
        {
            Id = GroupId,
            Page = true,
            CheckYourAnswers = true,
            SummaryTitle = "SummaryTitle"
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
    public async Task
        GetNextQuestion_ShouldReturnNextQuestionAlternative_WhenAnswerIsNoForYesNoQuestionAndNextQuestionAlternativeExists()
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
                new FormQuestion
                {
                    Id = currentQuestionId, NextQuestion = nextQuestionId,
                    NextQuestionAlternative = nextQuestionAlternativeId, Type = FormQuestionType.YesOrNo
                },
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

        var result =
            await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(nextQuestionAlternativeId);
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

        var result = await _formsEngine.GetPreviousQuestion(organisationId, formId, sectionId, currentQuestionId, null);

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
    public async Task
        GetFormSectionAsync_ShouldFetchChoicesFromCustomChoiceProvider_WhenCustomChoiceProviderIsConfigured()
    {
        var (organisationId, formId, sectionId, sessionKey) = CreateTestGuids();
        var questionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();
        var apiResponse = CreateApiSectionQuestionsResponse(sectionId, questionId, nextQuestionId,
            "ExclusionAppliesToChoiceProviderStrategy", "JsonValue");
        var expectedResponse = CreateModelSectionQuestionsResponse(sectionId, questionId, nextQuestionId,
            "ExclusionAppliesToChoiceProviderStrategy");

        expectedResponse.Questions[0].Options.Choices = new Dictionary<string, string>()
        {
            { $@"{{""id"":""{organisationId}"",""type"":""organisation""}}", "User's current organisation" },
            {
                "{\"id\":\"e4bdd7ef-8200-4257-9892-b16f43d1803e\",\"type\":\"connected-entity\"}",
                "First name Last name"
            },
            {
                "{\"id\":\"4c8dccba-df39-4997-814b-7599ed9b5bed\",\"type\":\"connected-entity\"}",
                "Connected organisation"
            }
        };

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

        expectedResponse.Questions[0].Options.Grouping = new FormQuestionGrouping
        {
            Id = GroupId,
            Page = true,
            CheckYourAnswers = true,
            SummaryTitle = "SummaryTitle"
        };

        var connectedIndividualGuid = new Guid("e4bdd7ef-8200-4257-9892-b16f43d1803e");

        _organisationClientMock.Setup(c => c.GetConnectedEntitiesAsync(It.IsAny<Guid>()))
            .ReturnsAsync([
                new ConnectedEntityLookup(endDate: null, entityId: connectedIndividualGuid,
                    entityType: ConnectedEntityType.Individual, name: "Connected person",
                    uri: new Uri("http://whatever"), deleted: false, isInUse: false, formGuid: null, sectionGuid: null),
                new ConnectedEntityLookup(endDate: null, entityId: new Guid("4c8dccba-df39-4997-814b-7599ed9b5bed"),
                    entityType: ConnectedEntityType.Organisation, name: "Connected organisation",
                    uri: new Uri("http://whatever"), deleted: false, isInUse: false, formGuid: null, sectionGuid: null)
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
            .ReturnsAsync(new Organisation.WebApiClient.Organisation(additionalIdentifiers: [], addresses: [],
                contactPoint: null, id: organisationId, identifier: null, name: "User's current organisation",
                type: OrganisationType.Organisation, roles: [],
                details: new Details(approval: null, buyerInformation: null, pendingRoles: [],
                    publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null)));
        _userInfoServiceMock.Setup(u => u.GetOrganisationId()).Returns(organisationId);
        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(sessionKey))
            .Returns((SectionQuestionsResponse?)null);
        _choiceProviderServiceMock.Setup(t => t.GetStrategy("ExclusionAppliesToChoiceProviderStrategy"))
            .Returns(new ExclusionAppliesToChoiceProviderStrategy(_userInfoServiceMock.Object,
                _organisationClientMock.Object));
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
                    (!expectedAnswer.NumericValue.HasValue || (a.NumericValue.HasValue && Math.Abs(a.NumericValue.Value - expectedAnswer.NumericValue.Value) < Tolerance)) &&
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
                    (!expectedAnswer.NumericValue.HasValue || (a.NumericValue.HasValue && Math.Abs(a.NumericValue.Value - expectedAnswer.NumericValue.Value) < Tolerance)) &&
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
                It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<WebApiClient.UpdateFormSectionAnswers>()))
            .ThrowsAsync(new Exception("API call failed"));

        Func<Task> act = async () => await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);
        await act.Should().ThrowAsync<Exception>().WithMessage("API call failed");
    }

    [Fact]
    public async Task SaveUpdateAnswers_WhenYesNoChangesToNo_ShouldBeCalledWithPreClearedMainPathAnswers()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();

        var yesNoQuestionId = Guid.NewGuid();
        var mainPathQuestionId1 = Guid.NewGuid();
        var mainPathQuestionId2 = Guid.NewGuid();
        var altPathQuestionId = Guid.NewGuid();

        var answerSet = new FormQuestionAnswerState
        {
            AnswerSetId = answerSetId,
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = yesNoQuestionId, Answer = new FormAnswer { BoolValue = false } },
                new QuestionAnswer
                {
                    QuestionId = altPathQuestionId, Answer = new FormAnswer { TextValue = "Correct Alt Path Answer" }
                }
            }
        };

        await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);

        _formsApiClientMock.Verify(api => api.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            answerSetId,
            organisationId,
            It.Is<WebApiClient.UpdateFormSectionAnswers>(payload =>
                payload.Answers.Count == 2 &&
                payload.Answers.Any(a => a.QuestionId == yesNoQuestionId && a.BoolValue == false) &&
                payload.Answers.Any(a =>
                    a.QuestionId == altPathQuestionId && a.TextValue == "Correct Alt Path Answer") &&
                !payload.Answers.Any(a => a.QuestionId == mainPathQuestionId1) &&
                !payload.Answers.Any(a => a.QuestionId == mainPathQuestionId2)
            )
        ), Times.Once);
    }

    [Fact]
    public async Task SaveUpdateAnswers_WhenFileUploadChangesToNoFile_ShouldBeCalledWithPreClearedMainPathAnswers()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();

        var fileUploadQuestionId = Guid.NewGuid();
        var mainPathQuestionId = Guid.NewGuid();
        var altPathQuestionId = Guid.NewGuid();

        var answerSet = new FormQuestionAnswerState
        {
            AnswerSetId = answerSetId,
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = fileUploadQuestionId, Answer = new FormAnswer { BoolValue = false, TextValue = null }
                },
                new QuestionAnswer
                {
                    QuestionId = altPathQuestionId, Answer = new FormAnswer { TextValue = "Correct Alt Path Answer" }
                }
            }
        };

        await _formsEngine.SaveUpdateAnswers(formId, sectionId, organisationId, answerSet);

        _formsApiClientMock.Verify(api => api.PutFormSectionAnswersAsync(
            formId,
            sectionId,
            answerSetId,
            organisationId,
            It.Is<WebApiClient.UpdateFormSectionAnswers>(payload =>
                payload.Answers.Count == 2 &&
                payload.Answers.Any(a =>
                    a.QuestionId == fileUploadQuestionId && a.BoolValue == false && a.TextValue == null) &&
                payload.Answers.Any(a =>
                    a.QuestionId == altPathQuestionId && a.TextValue == "Correct Alt Path Answer") &&
                !payload.Answers.Any(a => a.QuestionId == mainPathQuestionId)
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
        var question1 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question2 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question3 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        question1.NextQuestion = question2.Id;
        question2.NextQuestion = question3.Id;

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = question1.Id, Answer = new FormAnswer { TextValue = "Ans1" } },
                new QuestionAnswer { QuestionId = question2.Id, Answer = new FormAnswer { TextValue = "Ans2" } }
            }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().BeNull("because all questions on the path before question3 have been answered");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnFirstQuestion_WhenNoQuestionsAreAnswered()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question2 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question3 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        question1.NextQuestion = question2.Id;
        question2.NextQuestion = question3.Id;

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>()
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().Be(question1.Id,
            "because no questions have been answered, so the first question on the path should be returned");
    }


    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnExpectedResult_WhenCalled()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question2 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question3 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        question1.NextQuestion = question2.Id;
        question2.NextQuestion = question3.Id;

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = question2.Id, Answer = new FormAnswer { TextValue = "Ans2" } }
            }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().Be(question1.Id, "because question1 is the first unanswered question on the path");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldReturnNull_WhenAllPreviousQuestionsAreAnswered()
    {
        var question1 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question2 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var question3 = new FormQuestion { Id = Guid.NewGuid(), Type = FormQuestionType.Text };
        var questions = new List<FormQuestion> { question1, question2, question3 };

        question1.NextQuestion = question2.Id;
        question2.NextQuestion = question3.Id;

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = question1.Id, Answer = new FormAnswer { TextValue = "Ans1" } },
                new QuestionAnswer { QuestionId = question2.Id, Answer = new FormAnswer { TextValue = "Ans2" } }
            }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, question3.Id, answerState);

        result.Should().BeNull("because all questions on the path before question3 are answered");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_ShouldNotStartSortedListWithBranchedQuestion_WhenCalled()
    {
        var qActualStart = new FormQuestion
        {
            Id = Guid.NewGuid(), Title = "Actual Start", Type = FormQuestionType.Text,
            Options = new FormQuestionOptions()
        };
        var qAlternativeOnlyTarget = new FormQuestion
        {
            Id = Guid.NewGuid(), Title = "Alternative Only Target", Type = FormQuestionType.Text,
            Options = new FormQuestionOptions()
        };
        var qIntermediate = new FormQuestion
        {
            Id = Guid.NewGuid(), Title = "Intermediate", Type = FormQuestionType.Text,
            Options = new FormQuestionOptions()
        };
        var qEnd = new FormQuestion { Id = Guid.NewGuid(), Title = "End", Options = new FormQuestionOptions() };

        qActualStart.NextQuestion = qIntermediate.Id;
        qIntermediate.NextQuestion = qEnd.Id;
        qIntermediate.NextQuestionAlternative = qAlternativeOnlyTarget.Id;

        var questions = new List<FormQuestion> { qAlternativeOnlyTarget, qActualStart, qIntermediate, qEnd };
        var answerState = new FormQuestionAnswerState { Answers = new List<QuestionAnswer>() };
        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, qIntermediate.Id, answerState);

        result.Should().Be(qActualStart.Id);
    }

    [Fact]
    public void
        GetPreviousUnansweredQuestionId_PathViaNoOnYesNo_UnansweredOnYesPath_NoUnansweredOnNoPath_ShouldReturnNull()
    {
        var qStartId = Guid.NewGuid();
        var qBranchId = Guid.NewGuid();
        var qYesPathId = Guid.NewGuid();
        var qYesEndId = Guid.NewGuid();
        var qNoPathId = Guid.NewGuid();
        var qCurrentId = Guid.NewGuid();

        var questions = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = qStartId, Type = FormQuestionType.NoInput, NextQuestion = qBranchId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = qBranchId, Type = FormQuestionType.YesOrNo, NextQuestion = qYesPathId,
                NextQuestionAlternative = qNoPathId, Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = qYesPathId, Type = FormQuestionType.Text, NextQuestion = qYesEndId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion { Id = qYesEndId, Type = FormQuestionType.NoInput, Options = new FormQuestionOptions() },
            new FormQuestion
            {
                Id = qNoPathId, Type = FormQuestionType.NoInput, NextQuestion = qCurrentId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion { Id = qCurrentId, Type = FormQuestionType.Text, Options = new FormQuestionOptions() }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = qStartId, Answer = new FormAnswer() },
                new QuestionAnswer { QuestionId = qBranchId, Answer = new FormAnswer { BoolValue = false } },
                new QuestionAnswer { QuestionId = qNoPathId, Answer = new FormAnswer() }
            }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, qCurrentId, answerState);

        result.Should()
            .BeNull(
                "because the taken path (Q_Start -> Q_Branch -> Q_NoPath) has no unanswered questions requiring an answer, and Q_YesPath (unanswered) is not on this path");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_PathViaNoOnYesNo_UnansweredOnNoPath_ShouldReturnIt()
    {
        var qStartId = Guid.NewGuid();
        var qBranchId = Guid.NewGuid();
        var qYesPathId = Guid.NewGuid();
        var qYesEndId = Guid.NewGuid();
        var qNoPathUnansweredId = Guid.NewGuid();
        var qCurrentId = Guid.NewGuid();

        var questions = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = qStartId, Type = FormQuestionType.NoInput, NextQuestion = qBranchId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = qBranchId, Type = FormQuestionType.YesOrNo, NextQuestion = qYesPathId,
                NextQuestionAlternative = qNoPathUnansweredId, Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = qYesPathId, Type = FormQuestionType.Text, NextQuestion = qYesEndId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion { Id = qYesEndId, Type = FormQuestionType.NoInput, Options = new FormQuestionOptions() },
            new FormQuestion
            {
                Id = qNoPathUnansweredId, Type = FormQuestionType.Text, NextQuestion = qCurrentId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion { Id = qCurrentId, Type = FormQuestionType.Text, Options = new FormQuestionOptions() }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = qStartId, Answer = new FormAnswer() },
                new QuestionAnswer { QuestionId = qBranchId, Answer = new FormAnswer { BoolValue = false } }
            }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, qCurrentId, answerState);

        result.Should().Be(qNoPathUnansweredId,
            "because Q_NoPathUnansweredId is the first unanswered question on the taken path before Q_Current");
    }

    private (Guid formId, Guid sectionId, Guid organisationId, FormQuestionAnswerState answerSet, FormAnswer
        expectedAnswer) SetupTestData()
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

    [Fact]
    public void IsQuestionAnswered_NoInputQuestionExists_ShouldBeConsideredAnswered()
    {
        var noInputQuestionId = Guid.NewGuid();
        var noInputQuestion = new FormQuestion
        {
            Id = noInputQuestionId,
            Type = FormQuestionType.NoInput,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = noInputQuestionId,
                    AnswerId = Guid.NewGuid(),
                    Answer = new FormAnswer()
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { noInputQuestion, answerState });

        result.Should()
            .BeTrue("because NoInput questions should be considered answered when they exist in the answer collection");
    }

    [Fact]
    public void IsQuestionAnswered_NoInputQuestionNotExists_ShouldBeConsideredUnanswered()
    {
        var noInputQuestionId = Guid.NewGuid();
        var noInputQuestion = new FormQuestion
        {
            Id = noInputQuestionId,
            Type = FormQuestionType.NoInput,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>()
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { noInputQuestion, answerState });

        result.Should()
            .BeFalse(
                "because NoInput questions should be considered unanswered when they don't exist in the answer collection");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_NoInputQuestionUnanswered_ShouldReturnIt()
    {
        var qStartId = Guid.NewGuid();
        var qNoInputId = Guid.NewGuid();
        var qCurrentId = Guid.NewGuid();

        var questions = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = qStartId, Type = FormQuestionType.Text, NextQuestion = qNoInputId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = qNoInputId, Type = FormQuestionType.NoInput, NextQuestion = qCurrentId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion { Id = qCurrentId, Type = FormQuestionType.Text, Options = new FormQuestionOptions() }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = qStartId, Answer = new FormAnswer { TextValue = "Answered" } }
            }
        };
        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, qCurrentId, answerState);

        result.Should().Be(qNoInputId,
            "because the NoInput question is unanswered and should be returned as the first unanswered question");
    }

    [Fact]
    public void GetPreviousUnansweredQuestionId_NoInputQuestionAnswered_ShouldNotReturnIt()
    {
        var qStartId = Guid.NewGuid();
        var qNoInputId = Guid.NewGuid();
        var qCurrentId = Guid.NewGuid();

        var questions = new List<FormQuestion>
        {
            new FormQuestion
            {
                Id = qStartId, Type = FormQuestionType.Text, NextQuestion = qNoInputId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion
            {
                Id = qNoInputId, Type = FormQuestionType.NoInput, NextQuestion = qCurrentId,
                Options = new FormQuestionOptions()
            },
            new FormQuestion { Id = qCurrentId, Type = FormQuestionType.Text, Options = new FormQuestionOptions() }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer { QuestionId = qStartId, Answer = new FormAnswer { TextValue = "Answered" } },
                new QuestionAnswer { QuestionId = qNoInputId, Answer = new FormAnswer() }
            }
        };

        var result = _formsEngine.GetPreviousUnansweredQuestionId(questions, qCurrentId, answerState);

        result.Should().BeNull("because all questions in the path (including the NoInput question) are answered");
    }

    [Fact]
    public async Task GetPreviousQuestion_WhenOnAlternativePath_ReturnsCorrectPreviousQuestion()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var question1 = new FormQuestion { Id = Guid.NewGuid(), Title = "Question 1", Type = FormQuestionType.YesOrNo };
        var question2 = new FormQuestion { Id = Guid.NewGuid(), Title = "Question 2" };
        var question3 = new FormQuestion { Id = Guid.NewGuid(), Title = "Question 3" };

        question1.NextQuestion = question2.Id;
        question1.NextQuestionAlternative = question3.Id;

        var questions = new List<FormQuestion> { question1, question2, question3 };
        var sectionQuestionsResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions = questions
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new() { QuestionId = question1.Id, Answer = new FormAnswer { BoolValue = false } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionQuestionsResponse);

        var result =
            await _formsEngine.GetPreviousQuestion(organisationId, formId, sectionId, question3.Id, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(question1.Id);
    }

    [Fact]
    public void IsQuestionAnswered_RequiredQuestion_WithNullAnswer_ShouldReturnFalse()
    {
        var questionId = Guid.NewGuid();
        var textQuestion = new FormQuestion
        {
            Id = questionId,
            Type = FormQuestionType.Text,
            IsRequired = true,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new()
                {
                    QuestionId = questionId,
                    Answer = null
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { textQuestion, answerState });

        result.Should().BeFalse("because a required question with a null answer should be considered unanswered");
    }

    [Theory]
    [InlineData("option1", null, true, "SingleChoice with non-empty OptionValue should be considered answered")]
    [InlineData(null, "{\"value\":\"data\"}", true, "SingleChoice with non-empty JsonValue should be considered answered")]
    [InlineData("option1", "{\"value\":\"data\"}", true, "SingleChoice with both OptionValue and JsonValue should be considered answered")]
    [InlineData(null, null, false, "SingleChoice with neither OptionValue nor JsonValue should be considered unanswered")]
    [InlineData("", "", false, "SingleChoice with empty strings for both OptionValue and JsonValue should be considered unanswered")]
    public void IsQuestionAnswered_SingleChoiceQuestion_ReturnsExpectedResult(string? optionValue, string? jsonValue,
        bool expectedResult, string becauseMessage)
    {
        var questionId = Guid.NewGuid();
        var singleChoiceQuestion = new FormQuestion
        {
            Id = questionId,
            Type = FormQuestionType.SingleChoice,
            IsRequired = true,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new()
                {
                    QuestionId = questionId,
                    Answer = new FormAnswer
                    {
                        OptionValue = optionValue,
                        JsonValue = jsonValue
                    }
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { singleChoiceQuestion, answerState });

        result.Should().Be(expectedResult, becauseMessage);
    }

    [Theory]
    [InlineData(FormQuestionType.YesOrNo, true, "Yes answer",
        "should be able to navigate back from a question that follows a YesNoInput with Yes answer")]
    [InlineData(FormQuestionType.YesOrNo, false, "No answer",
        "should be able to navigate back from a question that follows a YesNoInput with No answer")]
    [InlineData(FormQuestionType.FileUpload, true, "file was uploaded",
        "should be able to navigate back from a question that follows a FileUpload with a file")]
    [InlineData(FormQuestionType.FileUpload, false, "no file was uploaded",
        "should be able to navigate back from a question that follows a FileUpload with no file")]
    public async Task GetPreviousQuestion_ShouldReturnPreviousQuestion_WhenOnBranchWithoutNextQuestionAlternative(
        FormQuestionType questionType,
        bool boolValue,
        string answerDescription,
        string becauseReason)
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var branchQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = branchQuestionId,
                    Type = questionType,
                    NextQuestion = nextQuestionId,
                    Title = $"{questionType} question"
                },
                new FormQuestion
                {
                    Id = nextQuestionId,
                    Type = FormQuestionType.Text,
                    Title = "Next question"
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                CreateQuestionAnswer(branchQuestionId, questionType, boolValue)
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result =
            await _formsEngine.GetPreviousQuestion(organisationId, formId, sectionId, nextQuestionId, answerState);

        result.Should().NotBeNull($"because previous navigation should work when {answerDescription}");
        result!.Id.Should().Be(branchQuestionId, $"because {becauseReason}");
        result.Type.Should().Be(questionType, $"because the previous question is of type {questionType}");
    }

    [Theory]
    [InlineData(FormQuestionType.YesOrNo, true, true,
        "Yes answer should go to NextQuestion even when NextQuestionAlternative exists")]
    [InlineData(FormQuestionType.YesOrNo, false, false,
        "No answer should go to NextQuestionAlternative when it exists")]
    [InlineData(FormQuestionType.FileUpload, true, true,
        "a file was uploaded and should go to NextQuestion even when NextQuestionAlternative exists")]
    [InlineData(FormQuestionType.FileUpload, false, false,
        "no file was uploaded and should go to NextQuestionAlternative when it exists")]
    public async Task GetNextQuestion_ShouldNavigateToCorrectQuestion_WhenNextQuestionAlternativeExists(
        FormQuestionType questionType,
        bool positiveAnswer,
        bool expectNextQuestion,
        string becauseReason)
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
                new FormQuestion
                {
                    Id = currentQuestionId,
                    Type = questionType,
                    NextQuestion = nextQuestionId,
                    NextQuestionAlternative = nextQuestionAlternativeId,
                    IsRequired = false
                },
                new FormQuestion { Id = nextQuestionId, Type = FormQuestionType.Text },
                new FormQuestion { Id = nextQuestionAlternativeId, Type = FormQuestionType.Text }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                CreateQuestionAnswer(currentQuestionId, questionType, positiveAnswer)
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result =
            await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull("because a next question should be returned");
        var expectedQuestionId = expectNextQuestion ? nextQuestionId : nextQuestionAlternativeId;
        result!.Id.Should().Be(expectedQuestionId, $"because {becauseReason}");
    }

    [Theory]
    [InlineData(FormQuestionType.YesOrNo, true,
        "Yes answer should go to NextQuestion when NextQuestionAlternative doesn't exist")]
    [InlineData(FormQuestionType.YesOrNo, false,
        "No answer should go to NextQuestion when NextQuestionAlternative doesn't exist")]
    [InlineData(FormQuestionType.FileUpload, true,
        "a file was uploaded and should go to NextQuestion when NextQuestionAlternative doesn't exist")]
    [InlineData(FormQuestionType.FileUpload, false,
        "no file was uploaded but should still go to NextQuestion when NextQuestionAlternative doesn't exist")]
    public async Task GetNextQuestion_ShouldNavigateToNextQuestion_WhenNextQuestionAlternativeDoesNotExist(
        FormQuestionType questionType,
        bool boolValue,
        string becauseReason)
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var nextQuestionId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = currentQuestionId,
                    Type = questionType,
                    NextQuestion = nextQuestionId,
                    IsRequired = false
                },
                new FormQuestion { Id = nextQuestionId, Type = FormQuestionType.Text }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                CreateQuestionAnswer(currentQuestionId, questionType, boolValue)
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result =
            await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull("because a next question should be returned");
        result!.Id.Should().Be(nextQuestionId, $"because {becauseReason}");
    }

    [Fact]
    public async Task GetNextQuestion_ShouldReturnNoInputQuestion_WhenNextQuestionIsNoInput()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var noInputQuestionId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = currentQuestionId,
                    Type = FormQuestionType.Text,
                    NextQuestion = noInputQuestionId,
                    IsRequired = false
                },
                new FormQuestion
                {
                    Id = noInputQuestionId,
                    Type = FormQuestionType.NoInput,
                    Title = "Information only question"
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = currentQuestionId,
                    Answer = new FormAnswer { TextValue = "Some text answer" }
                }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result =
            await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull("because a NoInput question should be returned as the next question");
        result!.Id.Should().Be(noInputQuestionId, "because the next question in the sequence is a NoInput question");
        result.Type.Should().Be(FormQuestionType.NoInput, "because the next question is of type NoInput");
    }

    [Fact]
    public async Task GetNextQuestion_ShouldReturnCheckYourAnswersQuestion_WhenNextQuestionIsCheckYourAnswers()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var currentQuestionId = Guid.NewGuid();
        var checkYourAnswersQuestionId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = currentQuestionId,
                    Type = FormQuestionType.Text,
                    NextQuestion = checkYourAnswersQuestionId,
                    IsRequired = false
                },
                new FormQuestion
                {
                    Id = checkYourAnswersQuestionId,
                    Type = FormQuestionType.CheckYourAnswers,
                    Title = "Check Your Answers"
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = currentQuestionId,
                    Answer = new FormAnswer { TextValue = "Some text answer" }
                }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result =
            await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull("because a CheckYourAnswers question should be returned as the next question");
        result!.Id.Should().Be(checkYourAnswersQuestionId,
            "because the next question in the sequence is a CheckYourAnswers question");
        result.Type.Should().Be(FormQuestionType.CheckYourAnswers,
            "because the next question is of type CheckYourAnswers");
    }

    [Fact]
    public async Task GetNextQuestion_YesNoInput_ShouldReturnNoInputQuestion_WhenNextQuestionIsNoInput()
    {
        var (organisationId, formId, sectionId, _) = CreateTestGuids();
        var yesNoQuestionId = Guid.NewGuid();
        var noInputQuestionId = Guid.NewGuid();

        var sectionResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "SectionTitle", AllowsMultipleAnswerSets = true },
            Questions = new List<FormQuestion>
            {
                new FormQuestion
                {
                    Id = yesNoQuestionId,
                    Type = FormQuestionType.YesOrNo,
                    NextQuestion = noInputQuestionId,
                    Title = "Yes/No question"
                },
                new FormQuestion
                {
                    Id = noInputQuestionId,
                    Type = FormQuestionType.NoInput,
                    Title = "Information only question"
                }
            }
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = yesNoQuestionId,
                    Answer = new FormAnswer { BoolValue = true }
                }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionResponse);

        var result =
            await _formsEngine.GetNextQuestion(organisationId, formId, sectionId, yesNoQuestionId, answerState);

        result.Should().NotBeNull("because a question should be returned after answering a YesNo question");
        result!.Id.Should().Be(noInputQuestionId,
            "because the next question after the YesNo question is a NoInput question");
        result.Type.Should().Be(FormQuestionType.NoInput, "because the next question is of type NoInput");
    }

    [Theory]
    [InlineData(FormQuestionType.YesOrNo, true)]
    [InlineData(FormQuestionType.YesOrNo, false)]
    [InlineData(FormQuestionType.FileUpload, true)]
    [InlineData(FormQuestionType.FileUpload, false)]
    public async Task GetPreviousQuestion_OnBranchingQuestion_ReturnsCorrectPreviousQuestion(
        FormQuestionType questionType, bool boolValue)
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var qBranch = new FormQuestion
        {
            Id = Guid.NewGuid(), Title = "Branching Question", Type = questionType, Options = new FormQuestionOptions()
        };
        var qYesPath = new FormQuestion { Id = Guid.NewGuid(), Title = "Yes Path" };
        var qNoPath = new FormQuestion { Id = Guid.NewGuid(), Title = "No Path" };

        qBranch.NextQuestion = qYesPath.Id;
        qBranch.NextQuestionAlternative = qNoPath.Id;

        var questions = new List<FormQuestion> { qBranch, qYesPath, qNoPath };
        var sectionQuestionsResponse = new SectionQuestionsResponse
        {
            Section = new FormSection { Id = sectionId, Title = "Test Section" },
            Questions = questions
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new() { QuestionId = qBranch.Id, Answer = new FormAnswer { BoolValue = boolValue } }
            }
        };

        _tempDataServiceMock.Setup(t => t.Peek<SectionQuestionsResponse>(It.IsAny<string>()))
            .Returns(sectionQuestionsResponse);

        var currentQuestionId = boolValue ? qYesPath.Id : qNoPath.Id;
        var because = boolValue
            ? "the 'yes' path was taken, so the branching question should be the previous one"
            : "the 'no' path was taken, so the branching question should be the previous one";

        var result =
            await _formsEngine.GetPreviousQuestion(organisationId, formId, sectionId, currentQuestionId, answerState);

        result.Should().NotBeNull();
        result!.Id.Should().Be(qBranch.Id, because);
    }

    [Theory]
    [InlineData(false, true, "TextValue", true, "a non-required file upload question with a file uploaded should be considered answered")]
    [InlineData(false, false, null, true, "a non-required file upload question with explicit false BoolValue should be considered answered")]
    [InlineData(true, true, "TextValue", true, "a required file upload question with a file uploaded should be considered answered")]
    [InlineData(true, false, null, false, "a required file upload question with explicit false BoolValue should not be considered answered")]
    [InlineData(true, null, null, false, "a required file upload question with no answer should not be considered answered")]
    [InlineData(false, null, null, false, "a non-required file upload question with no answer should not be considered answered")]
    [InlineData(false, null, "TextValue", true, "a non-required file upload question with TextValue but null BoolValue should be considered answered")]
    [InlineData(true, null, "TextValue", true, "a required file upload question with TextValue but null BoolValue should be considered answered")]
    public void IsQuestionAnswered_FileUploadQuestion_ReturnsExpectedResult(bool isRequired, bool? boolValue, string? textValue, bool expectedResult, string becauseMessage)
    {
        var questionId = Guid.NewGuid();
        var fileUploadQuestion = new FormQuestion
        {
            Id = questionId,
            Type = FormQuestionType.FileUpload,
            IsRequired = isRequired,
            Options = new FormQuestionOptions()
        };

        var answer = new FormAnswer { BoolValue = boolValue };
        if (textValue != null)
        {
            answer.TextValue = textValue;
        }

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = questionId,
                    Answer = answer
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { fileUploadQuestion, answerState });

        result.Should().Be(expectedResult, becauseMessage);
    }

    [Fact]
    public void IsQuestionAnswered_FileUploadQuestion_NoAnswer_ShouldReturnFalse()
    {
        var questionId = Guid.NewGuid();
        var fileUploadQuestion = new FormQuestion
        {
            Id = questionId,
            Type = FormQuestionType.FileUpload,
            IsRequired = true,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>()
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { fileUploadQuestion, answerState });

        result.Should().BeFalse("because a file upload question with no answer at all should be considered unanswered");
    }

    [Fact]
    public void IsQuestionAnswered_FileUploadQuestion_EmptyAnswer_ShouldReturnFalse()
    {
        var questionId = Guid.NewGuid();
        var fileUploadQuestion = new FormQuestion
        {
            Id = questionId,
            Type = FormQuestionType.FileUpload,
            IsRequired = true,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = questionId,
                    Answer = new FormAnswer()
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { fileUploadQuestion, answerState });

        result.Should().BeFalse("because a file upload question with an empty answer should be considered unanswered");
    }

    [Fact]
    public void IsQuestionAnswered_FileUploadQuestion_EmptyTextValue_ShouldReturnFalse()
    {
        var questionId = Guid.NewGuid();
        var fileUploadQuestion = new FormQuestion
        {
            Id = questionId,
            Type = FormQuestionType.FileUpload,
            IsRequired = true,
            Options = new FormQuestionOptions()
        };

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = questionId,
                    Answer = new FormAnswer { TextValue = "" }
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { fileUploadQuestion, answerState });

        result.Should().BeFalse("because a file upload question with an empty string TextValue should be considered unanswered");
    }

    [Theory]
    [InlineData(FormQuestionType.Text, false, false, null, true, "a non-required text question with explicit false BoolValue should be considered answered")]
    [InlineData(FormQuestionType.Text, true, false, null, false, "a required text question with explicit false BoolValue should not be considered answered")]
    [InlineData(FormQuestionType.Text, false, true, "some text", true, "a non-required text question with text should be considered answered")]
    [InlineData(FormQuestionType.Url, false, false, null, true, "a non-required url question with explicit false BoolValue should be considered answered")]
    [InlineData(FormQuestionType.Url, true, false, null, false, "a required url question with explicit false BoolValue should not be considered answered")]
    [InlineData(FormQuestionType.Url, false, true, "http://example.com", true, "a non-required url question with url should be considered answered")]
    [InlineData(FormQuestionType.Date, false, false, null, true, "a non-required date question with explicit false BoolValue should be considered answered")]
    [InlineData(FormQuestionType.Date, true, false, null, false, "a required date question with explicit false BoolValue should not be considered answered")]
    [InlineData(FormQuestionType.Date, false, true, "not null", true, "a non-required date question with date should be considered answered")] // "not null" will be used to set a date value
    public void IsQuestionAnswered_NonRequiredOptionalQuestions_ReturnsExpectedResult(FormQuestionType questionType, bool isRequired, bool? boolValue, string? textOrDateValue, bool expectedResult, string becauseMessage)
    {
        var questionId = Guid.NewGuid();
        var question = new FormQuestion
        {
            Id = questionId,
            Type = questionType,
            IsRequired = isRequired,
            Options = new FormQuestionOptions()
        };

        var answer = new FormAnswer { BoolValue = boolValue };
        if (textOrDateValue != null)
        {
            switch (questionType)
            {
                case FormQuestionType.Text:
                case FormQuestionType.Url:
                    answer.TextValue = textOrDateValue;
                    break;
                case FormQuestionType.Date:
                    answer.DateValue = DateTimeOffset.UtcNow;
                    break;
            }
        }

        var answerState = new FormQuestionAnswerState
        {
            Answers = new List<QuestionAnswer>
            {
                new QuestionAnswer
                {
                    QuestionId = questionId,
                    Answer = answer
                }
            }
        };

        var methodInfo = typeof(FormsEngine).GetMethod("IsQuestionAnswered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        methodInfo.Should().NotBeNull("because the private method 'IsQuestionAnswered' should be found");
        var result = (bool?)methodInfo!.Invoke(_formsEngine, new object[] { question, answerState });

        result.Should().Be(expectedResult, becauseMessage);
    }

    private static QuestionAnswer CreateQuestionAnswer(Guid questionId, FormQuestionType questionType, bool boolValue)
    {
        return questionType switch
        {
            FormQuestionType.YesOrNo => new QuestionAnswer
            {
                QuestionId = questionId,
                Answer = new FormAnswer { BoolValue = boolValue }
            },
            FormQuestionType.FileUpload => new QuestionAnswer
            {
                QuestionId = questionId,
                Answer = boolValue
                    ? new FormAnswer { TextValue = "sample-file.pdf" }
                    : new FormAnswer { BoolValue = false, TextValue = null }
            },
            _ => throw new ArgumentException($@"Unsupported question type: {questionType}", nameof(questionType))
        };
    }
}
