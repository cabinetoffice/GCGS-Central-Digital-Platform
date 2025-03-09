using QuestPDF.Infrastructure;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper
{
    using System;
    using System.Collections.Generic;
    using Xunit;
    using FluentAssertions;
    using Microsoft.AspNetCore.Mvc.Localization;
    using Moq;
    using CO.CDP.Localization;
    using CO.CDP.DataSharing.WebApi.AutoMapper;
    using CO.CDP.OrganisationInformation.Persistence.Forms;            
    using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;     

    public class FormQuestionOptionsResolverTests
    {
        [Fact]
        public void Resolve_ReturnsEmpty_WhenOptionsIsNull()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            localizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(key => new LocalizedHtmlString(key, key));

            var resolver = new FormQuestionOptionsResolver(localizerMock.Object);

            var question = new FormQuestionNonEf
            {
                Id = 1,
                Guid = Guid.NewGuid(),
                SortOrder = 1,
                IsRequired = false,
                Type = FormQuestionType.SingleChoice,
                Name = "Q1",
                Title = "Title1",
                Description = "Desc1",
                Options = null!, // Testing null scenario if your domain allows it
                Section = new FormSectionNonEf
                {
                    Id = 10,
                    Title = "SectionA",
                    Type = FormSectionType.Standard,
                    Questions = new List<FormQuestionNonEf>()
                }
            };

            var result = resolver.Resolve(question, null!, null!, null!);
            result.Should().BeEmpty();
        }

        [Fact]
        public void Resolve_ReturnsDynamic_WhenChoiceProviderStrategyIsNotEmpty()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            localizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(key => new LocalizedHtmlString(key, key));

            var resolver = new FormQuestionOptionsResolver(localizerMock.Object);

            var question = new FormQuestionNonEf
            {
                Id = 2,
                Guid = Guid.NewGuid(),
                SortOrder = 1,
                IsRequired = false,
                Type = FormQuestionType.SingleChoice,
                Name = "Q2",
                Title = "Title2",
                Description = "Desc2",
                Options = new FormQuestionOptions
                {
                    ChoiceProviderStrategy = "SomeStrategy"
                },
                Section = new FormSectionNonEf
                {
                    Id = 20,
                    Title = "SectionB",
                    Type = FormSectionType.Standard,
                    Questions = new List<FormQuestionNonEf>()
                }
            };

            var result = resolver.Resolve(question, null!, null!, null!);
            result.Should().HaveCount(1);
            result[0].Value.Should().Be("Dynamic");
        }

        [Fact]
        public void Resolve_ReturnsPlainChoices_WhenChoicesIsNotNull()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            localizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(key => new LocalizedHtmlString(key, key));

            var resolver = new FormQuestionOptionsResolver(localizerMock.Object);

            var question = new FormQuestionNonEf
            {
                Id = 3,
                Guid = Guid.NewGuid(),
                SortOrder = 1,
                IsRequired = false,
                Type = FormQuestionType.MultipleChoice,
                Name = "Q3",
                Title = "Title3",
                Description = "Desc3",
                Options = new FormQuestionOptions
                {
                    Choices = new List<FormQuestionChoice>
            {
                new FormQuestionChoice
                {
                    Id = Guid.NewGuid(),
                    Title = "Choice1",
                    GroupName = "",
                    Hint = new FormQuestionChoiceHint
                    {
                        // Fill in whichever fields your domain requires
                        Title = "",
                        Description = ""
                    }
                },
                new FormQuestionChoice
                {
                    Id = Guid.NewGuid(),
                    Title = "Choice2",
                    GroupName = "",
                    Hint = new FormQuestionChoiceHint
                    {
                        Title = "",
                        Description = ""
                    }
                }
            }
                },
                Section = new FormSectionNonEf
                {
                    Id = 30,
                    Title = "SectionC",
                    Type = FormSectionType.Standard,
                    Questions = new List<FormQuestionNonEf>()
                }
            };

            var result = resolver.Resolve(question, null!, null!, null!);
            result.Should().HaveCount(2);
            result[0].Value.Should().Be("Choice1");
            result[1].Value.Should().Be("Choice2");
        }

        [Fact]
        public void Resolve_ReturnsEmpty_WhenNoStrategyOrChoices()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            localizerMock
                .Setup(l => l[It.IsAny<string>()])
                .Returns<string>(key => new LocalizedHtmlString(key, key));

            var resolver = new FormQuestionOptionsResolver(localizerMock.Object);

            var question = new FormQuestionNonEf
            {
                Id = 4,
                Guid = Guid.NewGuid(),
                SortOrder = 1,
                IsRequired = false,
                Type = FormQuestionType.SingleChoice,
                Name = "Q4",
                Title = "Title4",
                Description = "Desc4",
                Options = new FormQuestionOptions
                {
                    // no ChoiceProviderStrategy
                    // no Choices
                },
                Section = new FormSectionNonEf
                {
                    Id = 40,
                    Title = "SectionD",
                    Type = FormSectionType.Standard,
                    Questions = new List<FormQuestionNonEf>()
                }
            };

            var result = resolver.Resolve(question, null!, null!, null!);
            result.Should().BeEmpty();
        }
    }
}
