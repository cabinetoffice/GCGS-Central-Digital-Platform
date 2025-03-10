using System.Collections.Generic;
using System.Text.Json;
using Xunit;
using FluentAssertions;
using CO.CDP.DataSharing.WebApi.AutoMapper;
using ModelFormAnswer = CO.CDP.DataSharing.WebApi.Model.FormAnswer;
using DomainFormQuestionType = CO.CDP.OrganisationInformation.Persistence.Forms.FormQuestionType;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;
using CO.CDP.OrganisationInformation.Persistence.Forms;

namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper
{
    public class JsonValueResolverTests
    {
        private readonly JsonValueResolver _resolver = new();

        [Fact]
        public void Resolve_ReturnsNull_WhenJsonValueIsEmpty()
        {
            var source = CreateFormAnswerNonEf(null);
            var destination = new ModelFormAnswer
            {
                QuestionName = "Q1"
            };

            var result = _resolver.Resolve(source, destination, null, null!);
            result.Should().BeNull();
        }

        [Fact]
        public void Resolve_ReturnsDictionary_WhenJsonValueIsValidJson()
        {
            var jsonString = "{\"key\":\"value\",\"number\":123}";
            var source = CreateFormAnswerNonEf(jsonString);
            var destination = new ModelFormAnswer
            {
                QuestionName = "Q2"
            };

            var result = _resolver.Resolve(source, destination, null, null!);
            result.Should().NotBeNull();

            var dict = result!;
            dict.Should().ContainKey("key");
            dict.Should().ContainKey("number");

            var keyElement = (JsonElement)dict["key"];
            keyElement.GetString().Should().Be("value");

            var numberElement = (JsonElement)dict["number"];
            numberElement.GetInt32().Should().Be(123);
        }

        private static FormAnswerNonEf CreateFormAnswerNonEf(string? jsonValue)
        {
            return new FormAnswerNonEf
            {
                QuestionId = 101,
                FormAnswerSetId = 201,
                JsonValue = jsonValue,
                Question = new FormQuestionNonEf
                {
                    Id = 301,
                    Guid = System.Guid.NewGuid(),
                    SortOrder = 1,
                    Type = DomainFormQuestionType.Text,
                    IsRequired = false,
                    Name = "TestQuestion",
                    Title = "TestTitle",
                    Description = null,
                    Options = new FormQuestionOptions(),
                    Section = new FormSectionNonEf
                    {
                        Id = 401,
                        Title = "SectionTitle",
                        Type = FormSectionType.Standard,
                        Questions = new List<FormQuestionNonEf>()
                    }
                },
                FormAnswerSet = new FormAnswerSetNonEf
                {
                    Id = 501,
                    Guid = System.Guid.NewGuid(),
                    SectionId = 601,
                    Section = new FormSectionNonEf
                    {
                        Id = 701,
                        Title = "SectionTitle2",
                        Type = FormSectionType.Standard,
                        Questions = new List<FormQuestionNonEf>()
                    },
                    Answers = new List<FormAnswerNonEf>()
                }
            };
        }
    }
}
