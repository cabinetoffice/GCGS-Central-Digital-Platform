using FluentAssertions;
using Moq;
using CO.CDP.Organisation.WebApi.Model;
using Persistence = CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class SupportUpdateOrganisationUseCaseTests
{
    private readonly Mock<Persistence.IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<Persistence.IPersonRepository> _mockPersonRepository;
    private readonly SupportUpdateOrganisationUseCase _useCase;
    private readonly Persistence.Organisation _organisation;
    private readonly Persistence.Person _person;

    public SupportUpdateOrganisationUseCaseTests()
    {
        _mockOrganisationRepository = new Mock<Persistence.IOrganisationRepository>();
        _mockPersonRepository = new Mock<Persistence.IPersonRepository>();
        _useCase = new SupportUpdateOrganisationUseCase(_mockOrganisationRepository.Object, _mockPersonRepository.Object);
        _organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = new Guid(),
            Tenant = null,
            Name = null
        };

        _person = new OrganisationInformation.Persistence.Person
        {
            Id = 1,
            Guid = new Guid(),
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com"
        };
    }

    [Fact]
    public async Task Execute_WhenOrganisationIsUnknown_ShouldThrowUnknownOrganisationException()
    {
        var organisationId = Guid.NewGuid();
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = Guid.NewGuid(),
                Approved = true,
                Comment = "Reviewed"
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(null as OrganisationInformation.Persistence.Organisation);

        Func<Task> action = async () => await _useCase.Execute((organisationId, supportUpdateOrganisation));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_WhenPersonIsUnknown_ShouldThrowUnknownPersonException()
    {;
        var personId = Guid.NewGuid();
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = personId,
                Approved = true,
                Comment = "Reviewed"
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(personId))
            .ReturnsAsync(null as Persistence.Person);

        Func<Task> action = async () => await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        await action.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {personId}.");
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndApproved_ShouldUpdateOrganisationReviewDetails()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = true,
                Comment = "Reviewed and approved"
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        _organisation.ApprovedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        _organisation.ReviewedBy.Should().Be(_person);
        _organisation.ReviewComment.Should().Be("Reviewed and approved");

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenUpdateIsReviewAndNotApproved_ShouldNotSetApprovalDateButStillSave()
    {
        var supportUpdateOrganisation = new SupportUpdateOrganisation
        {
            Organisation = new SupportOrganisationInfo
            {
                ReviewedById = _person.Guid,
                Approved = false,
                Comment = "Reviewed but rejected"
            },
            Type = SupportOrganisationUpdateType.Review
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute((_organisation.Guid, supportUpdateOrganisation));

        result.Should().BeTrue();
        _organisation.ApprovedOn.Should().BeNull();
        _organisation.ReviewedBy.Should().Be(_person);
        _organisation.ReviewComment.Should().Be("Reviewed but rejected");

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }
}
