using CO.CDP.Testcontainers.PostgreSql;
using FluentAssertions;

namespace CO.CDP.OrganisationInformation.Persistence.Tests;

public class DatabaseFormRepositoryTest(PostgreSqlFixture postgreSql) : IClassFixture<PostgreSqlFixture>
{
    [Fact]
    public async Task GetSectionAsync_WhenSectionDoesNotExist_ReturnsNull()
    {
        using var repository = FormRepository();

        var foundSection = await repository.GetSectionAsync(Guid.NewGuid(),Guid.NewGuid());

        foundSection.Should().BeNull();
    }

    [Fact]
    public async Task GetQuestionsAsync_WhenSectionDoesNotExist_ReturnsEmptyList()
    {
        using var repository = FormRepository();

        var nonExistentSectionId = Guid.NewGuid();

        var foundQuestions = await repository.GetQuestionsAsync(nonExistentSectionId);

        foundQuestions.Should().BeEmpty();
    }

    private IFormRepository FormRepository()
    {
        return new DatabaseFormRepository(postgreSql.OrganisationInformationContext());
    }
}
