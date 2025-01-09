using FluentAssertions;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using CO.CDP.DataSharing.WebApi.Model;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper;

public class CustomFormQuestionTypeResolverTests
{
    private readonly CustomFormQuestionTypeResolver _resolver;

    public CustomFormQuestionTypeResolverTests()
    {
        _resolver = new CustomFormQuestionTypeResolver();
    }

    [Theory]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.YesOrNo, FormQuestionType.Boolean)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.CheckBox, FormQuestionType.Boolean)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.Text, FormQuestionType.Text)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.MultiLine, FormQuestionType.Text)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.FileUpload, FormQuestionType.FileUpload)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.Address, FormQuestionType.Text)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.SingleChoice, FormQuestionType.Option)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.SingleChoice, FormQuestionType.OptionJson, "JsonValue")]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.MultipleChoice, FormQuestionType.Option)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.GroupedSingleChoice, FormQuestionType.Option)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.Date, FormQuestionType.Date)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.Url, FormQuestionType.Url)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.NoInput, FormQuestionType.None)]
    [InlineData(OrganisationInformation.Persistence.Forms.FormQuestionType.CheckYourAnswers, FormQuestionType.None)]
    public void Resolve_ReturnsExpectedFormQuestionType(
        OrganisationInformation.Persistence.Forms.FormQuestionType sourceType,
        FormQuestionType expectedType,
        string? answerFieldName = null)
    {
        OrganisationInformation.Persistence.Forms.FormQuestion sourceQuestion = GivenQuestion(sourceType, answerFieldName);

        var result = _resolver.Resolve(sourceQuestion, null!, default, null!);

        result.Should().Be(expectedType);
    }

    [Fact]
    public void Resolve_ShouldHandleAllFormQuestionTypesWithoutException()
    {
        foreach (OrganisationInformation.Persistence.Forms.FormQuestionType formQuestionType in Enum.GetValues(typeof(OrganisationInformation.Persistence.Forms.FormQuestionType)))
        {
            OrganisationInformation.Persistence.Forms.FormQuestion sourceQuestion = GivenQuestion(formQuestionType);
            Action act = () => _resolver.Resolve(sourceQuestion, null!, default, null!);
            act.Should().NotThrow();
        }
    }

    private static OrganisationInformation.Persistence.Forms.FormQuestion GivenQuestion(OrganisationInformation.Persistence.Forms.FormQuestionType type, string? answerFieldName = null)
    {
        return new OrganisationInformation.Persistence.Forms.FormQuestion {
            Type = type,
            Guid = Guid.NewGuid(),
            NextQuestion = null,
            NextQuestionAlternative = null,
            Caption = null,
            Description = null,
            IsRequired = false,
            CreatedOn = DateTime.Now,
            Name = null!,
            Options = new () {
                AnswerFieldName = answerFieldName
            },
            Section = null!,
            SortOrder = 1,
            Title = null!,
        };
    }
}
