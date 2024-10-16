using AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetOrganisationJoinRequestUseCaseTests
{
    private readonly Mock<IOrganisationJoinRequestRepository> _requestRepositoryMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetOrganisationJoinRequestUseCase _useCase;

    public GetOrganisationJoinRequestUseCaseTests()
    {
        _requestRepositoryMock = new Mock<IOrganisationJoinRequestRepository>();
        _mapperMock = new Mock<IMapper>();
        _useCase = new GetOrganisationJoinRequestUseCase(_requestRepositoryMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Execute_ReturnsMappedOrganisationJoinRequests_WhenStatusIsNull()
    {
        var organisationId = Guid.NewGuid();
        var joinRequestId = Guid.NewGuid();

        var joinRequests = new List<OrganisationJoinRequest>
        {
            new OrganisationJoinRequest { Guid = joinRequestId, Status = OrganisationJoinRequestStatus.Pending }
        };

        var mappedRequests = new List<Model.OrganisationJoinRequest>
        {
            new Model.OrganisationJoinRequest
            {
                Id = joinRequestId,
                Organisation = It.IsAny<Model.Organisation>(),
                Person = It.IsAny<Model.Person>(),
                ReviewedBy = It.IsAny<Model.Person>(),
                ReviewedOn = DateTime.UtcNow,
                Status = OrganisationJoinRequestStatus.Pending
            }
        };

        _requestRepositoryMock
            .Setup(r => r.FindByOrganisation(organisationId))
            .ReturnsAsync(joinRequests);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Model.OrganisationJoinRequest>>(joinRequests))
            .Returns(mappedRequests);

        var result = await _useCase.Execute((organisationId, null));

        result.Should().BeEquivalentTo(mappedRequests);
        _requestRepositoryMock.Verify(r => r.FindByOrganisation(organisationId), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<Model.OrganisationJoinRequest>>(joinRequests), Times.Once);
    }

    [Fact]
    public async Task Execute_ReturnsFilteredMappedOrganisationJoinRequests_WhenStatusIsProvided()
    {
        var organisationId = Guid.NewGuid();
        var status = OrganisationJoinRequestStatus.Accepted;

        var joinRequests = new List<OrganisationJoinRequest>
        {
            new OrganisationJoinRequest { Guid = Guid.NewGuid(), Status = OrganisationJoinRequestStatus.Pending },
            new OrganisationJoinRequest { Guid = Guid.NewGuid(), Status = OrganisationJoinRequestStatus.Accepted }
        };

        var filteredRequests = joinRequests.Where(o => o.Status == status).ToList();

        var mappedRequests = new List<Model.OrganisationJoinRequest>
        {
            new Model.OrganisationJoinRequest
            {
                Id = filteredRequests[0].Guid,
                Organisation = It.IsAny<Model.Organisation>(),
                Person = It.IsAny<Model.Person>(),
                ReviewedBy = It.IsAny<Model.Person>(),
                ReviewedOn = DateTime.UtcNow,
                Status = OrganisationJoinRequestStatus.Accepted
            }
        };

        _requestRepositoryMock
            .Setup(r => r.FindByOrganisation(organisationId))
            .ReturnsAsync(joinRequests);

        _mapperMock
            .Setup(m => m.Map<IEnumerable<Model.OrganisationJoinRequest>>(filteredRequests))
            .Returns(mappedRequests);

        var result = await _useCase.Execute((organisationId, status));

        result.Should().BeEquivalentTo(mappedRequests);
        _requestRepositoryMock.Verify(r => r.FindByOrganisation(organisationId), Times.Once);
        _mapperMock.Verify(m => m.Map<IEnumerable<Model.OrganisationJoinRequest>>(filteredRequests), Times.Once);
    }
}