using CO.CDP.Forms.WebApi.Model;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.TestKit.Mvc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using static System.Net.HttpStatusCode;

namespace CO.CDP.Forms.WebApi.Tests.Api;

public class GetFormSectionQuestionsTest
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IUseCase<(Guid formId, Guid sectionId, Guid organisationId), SectionQuestionsResponse?>> _getFormSectionQuestionsUseCase = new();

    public GetFormSectionQuestionsTest()
    {
        TestWebApplicationFactory<Program> factory = new(builder =>
        {
            builder.ConfigureServices(services =>
                services.AddScoped<IUseCase<(Guid formId, Guid sectionId, Guid organisationId), SectionQuestionsResponse?>>(_ => _getFormSectionQuestionsUseCase.Object)
            );
        });
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task ItDoesNotFindTheFormSection()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var tuple = (formId, sectionId, organisationId);

        _getFormSectionQuestionsUseCase.Setup(useCase => useCase.Execute(tuple))
            .ReturnsAsync((SectionQuestionsResponse?)null);

        var response = await _httpClient.GetAsync($"/forms/{formId}/sections/{sectionId}/questions?organisation-id={organisationId}");

        response.Should().HaveStatusCode(NotFound);
    }

    [Fact]
    public async Task ItFindsTheFormSectionWithQuestions()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();
        var tuple = (formId, sectionId, organisationId);
        var sectionQuestionsResponse = GivenSectionQuestionsResponse();

        _getFormSectionQuestionsUseCase.Setup(useCase => useCase.Execute(tuple))
                .ReturnsAsync(sectionQuestionsResponse);

        var response = await _httpClient.GetAsync($"/forms/{formId}/sections/{sectionId}/questions?organisation-id={organisationId}");

        response.Should().HaveStatusCode(OK);
        await response.Should().HaveContent(sectionQuestionsResponse);
    }

    private static SectionQuestionsResponse GivenSectionQuestionsResponse()
    {
        var question1 = Guid.NewGuid();
        var question2 = Guid.NewGuid();
        var question3 = Guid.NewGuid();
        var question4 = Guid.NewGuid();
        var question5 = Guid.NewGuid();
        var question6 = Guid.NewGuid();

        return new SectionQuestionsResponse
        {
            Section = new FormSection
            {
                Id = Guid.NewGuid(),
                Title = "Financial Information",
                AllowsMultipleAnswerSets = true,
                Configuration = new FormSectionConfiguration
                {
                    PluralSummaryHeadingFormat = "You have added {0} files",
                    SingularSummaryHeading = "You have added 1 file",
                    AddAnotherAnswerLabel = "Add another file?",
                    RemoveConfirmationCaption = "Economic and Financial Standing",
                    RemoveConfirmationHeading = "Are you sure you want to remove this file?"
                }
            },
            Questions = new List<FormQuestion>
                {
                    new FormQuestion
                    {
                        Id = question1,
                        Title = "The financial information you will need.",
                        Description = "You will need to upload accounts or statements for your 2 most recent financial years. If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date for the information you upload.",
                        Type = FormQuestionType.NoInput,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = question2,
                        Title = "Were your accounts audited?",
                        Type = FormQuestionType.YesOrNo,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = question3,
                        Title = "Upload your accounts",
                        Description = "Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.",
                        Type = FormQuestionType.FileUpload,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = question4,
                        Title = "What is the financial year end date for the information you uploaded?",
                        Type = FormQuestionType.Date,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = question5,
                        Title = "Check your answers",
                        Type = FormQuestionType.CheckYourAnswers,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = question6,
                        Title = "Enter your postal address",
                        Type = FormQuestionType.Address,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    }
                },
            AnswerSets = new List<FormAnswerSet>
            {
                new FormAnswerSet
                {
                    Id= Guid.NewGuid(),
                    Answers= new List<FormAnswer>
                    {
                        new FormAnswer
                        {
                            Id=Guid.NewGuid(),
                            QuestionId=question1,
                            BoolValue=true
                        },
                          new FormAnswer
                        {
                            Id=Guid.NewGuid(),
                            QuestionId=question2,
                            OptionValue="yes"
                        },
                            new FormAnswer
                        {
                            Id=Guid.NewGuid(),
                            QuestionId=question3,
                            BoolValue=true
                        },
                        new FormAnswer
                        {
                            Id=Guid.NewGuid(),
                            QuestionId=question4,
                            DateValue=DateTime.Now
                        }
                    }
                }

            }

        };
    }
}