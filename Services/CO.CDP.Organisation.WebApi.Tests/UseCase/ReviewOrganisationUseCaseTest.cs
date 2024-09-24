using FluentAssertions;
using Moq;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class ReviewOrganisationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly ReviewOrganisationUseCase _useCase;
    private readonly ReviewOrganisation _command;
    private readonly OrganisationInformation.Persistence.Organisation _organisation;
    private readonly OrganisationInformation.Persistence.Person _person;

    public ReviewOrganisationUseCaseTest()
    {
        _mockOrganisationRepository = new Mock<IOrganisationRepository>();
        _mockPersonRepository = new Mock<IPersonRepository>();
        _useCase = new ReviewOrganisationUseCase(_mockOrganisationRepository.Object, _mockPersonRepository.Object);
        _command = new ReviewOrganisation
        {
            OrganisationId = Guid.NewGuid(),
            approvedById = Guid.NewGuid(),
            Approved = true,
            Comment = "Looks good"
        };
        _organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            Tenant = null,
            Name = null
        };
        _person = new Person
        {
            Id = 1,
            Guid = Guid.NewGuid(),
            FirstName = null,
            LastName = null,
            Email = null
        };
    }

    [Fact]
    public async Task Execute_WhenOrganisationIsUnknown_ShouldThrowUnknownOrganisationException()
    {
        _mockOrganisationRepository.Setup(repo => repo.Find(_command.OrganisationId))
            .ReturnsAsync((OrganisationInformation.Persistence.Organisation)null);

        Func<Task> action = async () => await _useCase.Execute(_command);

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_command.OrganisationId}.");
    }

    [Fact]
    public async Task Execute_WhenPersonIsUnknown_ShouldThrowUnknownPersonException()
    {
        _mockOrganisationRepository.Setup(repo => repo.Find(_command.OrganisationId))
            .ReturnsAsync(_organisation);
        _mockPersonRepository.Setup(repo => repo.Find(_command.approvedById))
            .ReturnsAsync((Person)null);

        Func<Task> action = async () => await _useCase.Execute(_command);

        await action.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {_command.approvedById}.");
    }

    [Fact]
    public async Task Execute_WhenApproved_ShouldUpdateOrganisationApprovalDetails()
    {
        _mockOrganisationRepository.Setup(repo => repo.Find(_command.OrganisationId))
            .ReturnsAsync(_organisation);
        _mockPersonRepository.Setup(repo => repo.Find(_command.approvedById))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute(_command);

        result.Should().BeTrue();
        _organisation.ApprovedOn.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
        _organisation.ApprovedBy.Should().Be(_person);
        _organisation.ApprovedComment.Should().Be(_command.Comment);

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_WhenNotApproved_ShouldNotSetApprovalDateButStillSave()
    {
        var rejectCommand = new ReviewOrganisation
        {
            OrganisationId = Guid.NewGuid(),
            approvedById = Guid.NewGuid(),
            Approved = false,
            Comment = "Nope, sorry"
        };

        _mockOrganisationRepository.Setup(repo => repo.Find(rejectCommand.OrganisationId))
            .ReturnsAsync(_organisation);
        _mockPersonRepository.Setup(repo => repo.Find(rejectCommand.approvedById))
            .ReturnsAsync(_person);

        var result = await _useCase.Execute(rejectCommand);

        result.Should().BeTrue();
        _organisation.ApprovedOn.Should().BeNull();
        _organisation.ApprovedBy.Should().Be(_person);
        _organisation.ApprovedComment.Should().Be(rejectCommand.Comment);

        _mockOrganisationRepository.Verify(repo => repo.Save(_organisation), Times.Once);
    }
}
