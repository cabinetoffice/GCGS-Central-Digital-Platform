using CO.CDP.Forms.WebApi.Tests.AutoMapper;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.OrganisationInformation.Persistence.Forms;
using FluentAssertions;
using Moq;

namespace CO.CDP.Forms.WebApi.Tests.UseCase;

public class GetFormSectionsUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IFormRepository> _repository = new();

    private GetFormSectionsUseCase _useCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_ShouldReturnNull_WhenNoFormSummariesFound()
    {
        var formId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        _repository.Setup(repo => repo.GetFormSummaryAsync(formId, organisationId))
                           .ReturnsAsync(new List<FormSectionSummary>());

        var result = await _useCase.Execute((formId, organisationId));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Execute_ShouldReturnFormSectionResponse_WhenFormSummariesAreFound()
    {
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        var organisationId = Guid.NewGuid();

        List<FormSectionSummary> formSummaries = [new FormSectionSummary {
            AllowsMultipleAnswerSets = true,
            AnswerSetCount = 1,
            Type = FormSectionType.Standard,
            SectionId = sectionId,
            SectionName = "TestSection" }];

        _repository.Setup(repo => repo.GetFormSummaryAsync(formId, organisationId))
                           .ReturnsAsync(formSummaries);

        var result = await _useCase.Execute((formId, organisationId));

        result.Should().NotBeNull();
        var resultFormSections = result.As<Model.FormSectionResponse>().FormSections;

        resultFormSections.Should().HaveCount(1);
        resultFormSections.First().SectionId.Should().Be(sectionId);
        resultFormSections.First().SectionName.Should().Be("TestSection");
    }
}
