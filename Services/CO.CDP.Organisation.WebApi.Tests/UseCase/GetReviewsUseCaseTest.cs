using Moq;
using FluentAssertions;
using AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetReviewsUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetReviewsUseCase _useCase;

    public GetReviewsUseCaseTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _mapperMock = new Mock<IMapper>();
        _useCase = new GetReviewsUseCase(_organisationRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldReturnMappedReview_WhenOrganisationExists()
    {
        var organisationId = Guid.NewGuid();
        var organisation = GivenOrganisation(organisationId);
        var expectedReview = GivenReview();

        _organisationRepositoryMock
            .Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        _mapperMock
            .Setup(mapper => mapper.Map<Review>(organisation))
            .Returns(expectedReview);

        var result = await _useCase.Execute(organisationId);

        result.Should().ContainSingle(review => review.Status == ReviewStatus.Approved);
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationDoesNotExist()
    {
        var organisationId = Guid.NewGuid();

        _organisationRepositoryMock
            .Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(null as OrganisationInformation.Persistence.Organisation);

        Func<Task> act = async () => await _useCase.Execute(organisationId);

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldCallRepositoryAndMapper_WithCorrectParameters()
    {
        var organisationId = Guid.NewGuid();
        var organisation = GivenOrganisation(organisationId);
        var expectedReview = GivenReview();

        _organisationRepositoryMock
            .Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        _mapperMock
            .Setup(mapper => mapper.Map<Review>(organisation))
            .Returns(expectedReview);

        await _useCase.Execute(organisationId);

        _organisationRepositoryMock.Verify(repo => repo.Find(organisationId), Times.Once);
        _mapperMock.Verify(mapper => mapper.Map<Review>(organisation), Times.Once);
    }

    private static Review GivenReview()
    {
        return new Review { Status = ReviewStatus.Approved };
    }

    private static OrganisationInformation.Persistence.Organisation GivenOrganisation(Guid organisationId)
    {
        return new OrganisationInformation.Persistence.Organisation {
            Id = 1,
            Guid = organisationId,
            Name = "Test Organisation",
            Tenant = new Tenant {
                Guid = new Guid(),
                Name = "Tenant"
            }
        };
    }
}
