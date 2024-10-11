using CO.CDP.AwsServices;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.DataSharing.WebApi.Tests.UseCase;

public class GetSharedDataDocumentDownloadUrlUseCaseTests
{
    private readonly Mock<IShareCodeRepository> _shareCodeRepositoryMock = new();
    private readonly Mock<IFileHostManager> _fileHostManagerMock = new();
    private readonly GetSharedDataDocumentDownloadUrlUseCase _useCase;

    public GetSharedDataDocumentDownloadUrlUseCaseTests()
    {
        _useCase = new GetSharedDataDocumentDownloadUrlUseCase(
            _shareCodeRepositoryMock.Object,
            _fileHostManagerMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnNull_WhenDocumentDoesNotExist()
    {
        _shareCodeRepositoryMock.Setup(x => x.ShareCodeDocumentExistsAsync("shareCode1", "documentId1")).ReturnsAsync(false);

        var result = await _useCase.Execute(("shareCode1", "documentId1"));

        result.Should().BeNull();
    }

    [Fact]
    public async Task Execute_ShouldReturnPresignedUrl_WhenDocumentExists()
    {
        _shareCodeRepositoryMock.Setup(x => x.ShareCodeDocumentExistsAsync("shareCode1", "documentId1")).ReturnsAsync(true);
        _fileHostManagerMock.Setup(x => x.GeneratePresignedUrl("documentId1", 10080)).ReturnsAsync("https://example.com/presignedurl");

        var result = await _useCase.Execute(("shareCode1", "documentId1"));

        result.Should().Be("https://example.com/presignedurl");
    }
}