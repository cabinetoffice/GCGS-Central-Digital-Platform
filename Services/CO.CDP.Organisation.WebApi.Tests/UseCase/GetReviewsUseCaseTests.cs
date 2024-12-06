using Moq;
using FluentAssertions;
using AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetReviewsUseCaseTests : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly GetReviewsUseCase _useCase;

    public GetReviewsUseCaseTests(AutoMapperFixture mapperFixture)
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new GetReviewsUseCase(_organisationRepositoryMock.Object, mapperFixture.Mapper);
    }

    [Fact]
    public async Task Execute_ShouldReturnMappedReview_WhenOrganisationExists()
    {
        var organisationId = Guid.NewGuid();
        var organisation = GivenOrganisation(organisationId);
        var expectedReview = GivenReview();

        _organisationRepositoryMock
            .Setup(repo => repo.FindIncludingReviewedBy(organisationId))
            .ReturnsAsync(organisation);

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
            Type = OrganisationInformation.OrganisationType.Organisation,
            Tenant = new Tenant {
                Guid = new Guid(),
                Name = "Tenant"
            }
        };
    }
}
