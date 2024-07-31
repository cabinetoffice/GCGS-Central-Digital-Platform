using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Forms.WebApi.Tests.UseCase;

public class DeleteAnswerSetUseCaseTest
{
    [Fact]
    public async Task Execute_ShouldReturnTrue_WhenDeletionIsSuccessful()
    {
        var formRepositoryMock = new Mock<IFormRepository>();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        formRepositoryMock
            .Setup(repo => repo.DeleteAnswerSetAsync(organisationId, answerSetId))
            .ReturnsAsync(true);

        var useCase = new DeleteAnswerSetUseCase(formRepositoryMock.Object);

        var result = await useCase.Execute((organisationId, answerSetId));

        result.Should().BeTrue();
        formRepositoryMock.Verify(repo => repo.DeleteAnswerSetAsync(organisationId, answerSetId), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldReturnFalse_WhenDeletionFails()
    {
        var formRepositoryMock = new Mock<IFormRepository>();
        var organisationId = Guid.NewGuid();
        var answerSetId = Guid.NewGuid();
        formRepositoryMock
            .Setup(repo => repo.DeleteAnswerSetAsync(organisationId, answerSetId))
            .ReturnsAsync(false);

        var useCase = new DeleteAnswerSetUseCase(formRepositoryMock.Object);

        var result = await useCase.Execute((organisationId, answerSetId));

        result.Should().BeFalse();
        formRepositoryMock.Verify(repo => repo.DeleteAnswerSetAsync(organisationId, answerSetId), Times.Once);
    }
}