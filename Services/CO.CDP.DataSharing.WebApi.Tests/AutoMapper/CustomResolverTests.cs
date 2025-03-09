using System;
using Xunit;
using FluentAssertions;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using Persistence = CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper
{
    public class CustomResolverTests
    {
        private readonly CustomResolver _resolver = new();

        [Theory]
        [InlineData(Persistence.FormQuestionType.Text, "Hello world", null, "Hello world")]
        [InlineData(Persistence.FormQuestionType.FileUpload, "some_file.pdf", null, "some_file.pdf")]
        [InlineData(Persistence.FormQuestionType.YesOrNo, null, true, "True")]
        [InlineData(Persistence.FormQuestionType.CheckBox, null, false, "False")]
        public void Resolve_MapsCorrectly_ForTextFileYesNoCheckbox(
            Persistence.FormQuestionType questionType,
            string? textValue,
            bool? boolValue,
            string expected)
        {
            var source = CreateQuestionAnswer(questionType);
            source.Answer.TextValue = textValue;
            source.Answer.BoolValue = boolValue;

            var result = _resolver.Resolve(source, null!, null, null!);
            result.Should().Be(expected);
        }

        [Fact]
        public void Resolve_MapsCorrectly_ForDate()
        {
            var date = DateTime.Today;
            var source = CreateQuestionAnswer(Persistence.FormQuestionType.Date);
            source.Answer.DateValue = date;

            var result = _resolver.Resolve(source, null!, null, null!);
            result.Should().Be(date.ToString());
        }

        [Fact]
        public void Resolve_MapsCorrectly_ForAddress()
        {
            var address = new FormAddress
            {
                StreetAddress = "123 Main St",
                Locality = "Townsville",
                PostalCode = "AB1 2CD",
                Region = "RegionX",
                CountryName = "Wonderland",
                Country = "WL"
            };

            var source = CreateQuestionAnswer(Persistence.FormQuestionType.Address);
            source.Answer.AddressValue = address;

            var result = _resolver.Resolve(source, null!, null, null!);
            result.Should().Be("123 Main St, Townsville, AB1 2CD, RegionX, Wonderland");
        }

        [Fact]
        public void Resolve_ReturnsEmpty_WhenQuestionTypeDoesNotMatchAnyCase()
        {
            var source = CreateQuestionAnswer((Persistence.FormQuestionType)9999);
            source.Answer.TextValue = "Unused text";

            var result = _resolver.Resolve(source, null!, null, null!);
            result.Should().Be("");
        }

        private Persistence.SharedConsentQuestionAnswer CreateQuestionAnswer(Persistence.FormQuestionType questionType)
        {
            return new Persistence.SharedConsentQuestionAnswer
            {
                Title = "TestTitle",
                QuestionType = questionType,
                Answer = new Persistence.FormAnswer
                {
                    Guid = Guid.NewGuid(),
                    QuestionId = 42,
                    FormAnswerSetId = 99,
                    Question = new Persistence.FormQuestion
                    {
                        Id = 123,
                        Guid = Guid.NewGuid(),
                        Title = "DummyTitle",
                        Caption = "DummyCaption",
                        Description = "DummyDescription",
                        Name = "DummyName",
                        Type = questionType,
                        IsRequired = false,
                        Options = new Persistence.FormQuestionOptions(),
                        SortOrder = 1,
                        NextQuestion = null,
                        NextQuestionAlternative = null,
                        Section = new Persistence.FormSection
                        {
                            Id = 999,
                            Guid = Guid.NewGuid(),
                            Title = "SectionTitle",
                            Type = FormSectionType.Exclusions,
                            FormId = 101,
                            AllowsMultipleAnswerSets = false,
                            CheckFurtherQuestionsExempted = false,
                            DisplayOrder = 1,
                            Configuration = new Persistence.FormSectionConfiguration
                            {
                                PluralSummaryHeadingFormat = "",
                                SingularSummaryHeading = "",
                                AddAnotherAnswerLabel = "",
                                RemoveConfirmationCaption = "",
                                RemoveConfirmationHeading = "",
                                FurtherQuestionsExemptedHeading = "",
                                FurtherQuestionsExemptedHint = ""
                            },
                            Questions = new System.Collections.Generic.List<Persistence.FormQuestion>(),
                            Form = new Persistence.Form
                            {
                                Guid = Guid.NewGuid(),
                                Name = "DummyForm",
                                Version = "1.0",
                                IsRequired = true,
                                Scope = FormScope.SupplierInformation,
                                Sections = new System.Collections.Generic.List<Persistence.FormSection>()
                            }
                        }
                    }
                }
            };
        }
    }
}
