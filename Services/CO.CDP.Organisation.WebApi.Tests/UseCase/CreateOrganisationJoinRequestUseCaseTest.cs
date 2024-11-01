using FluentAssertions;
using Moq;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using AutoMapper;
using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrganisationJoinRequest = CO.CDP.Organisation.WebApi.Model.OrganisationJoinRequest;
using Person = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class CreateOrganisationJoinRequestUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _mockOrganisationRepository;
    private readonly Mock<IPersonRepository> _mockPersonRepository;
    private readonly Mock<IOrganisationJoinRequestRepository> _mockOrganisationJoinRequestRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<IGovUKNotifyApiClient> _notifyApiClient = new();
    private readonly Mock<ILogger<CreateOrganisationJoinRequestUseCase>> _logger = new();
    private readonly CreateOrganisationJoinRequestUseCase _useCase;
    private readonly OrganisationInformation.Persistence.Organisation _organisation;
    private readonly Person _person;

    public CreateOrganisationJoinRequestUseCaseTests()
    {
        _mockOrganisationRepository = new Mock<IOrganisationRepository>();
        _mockPersonRepository = new Mock<IPersonRepository>();
        _mockOrganisationJoinRequestRepository = new Mock<IOrganisationJoinRequestRepository>();
        _mockMapper = new Mock<IMapper>();
        _organisation = new OrganisationInformation.Persistence.Organisation
        {
            Id = 1,
            Guid = new Guid(),
            Name = "Test organisation",
            Tenant = null!
        };

        _person = new Person
        {
            Id = 0,
            Guid = default,
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com"
        };

        var inMemorySettings = new List<KeyValuePair<string, string?>>
        {
            new("OrganisationAppUrl", "http://baseurl/"),
            new("GOVUKNotify:RequestToJoinNotifyOrgAdminTemplateId", "RequestToJoinNotifyOrgAdminTemplateId"),
            new("GOVUKNotify:RequestToJoinConfirmationEmailTemplateId", "RequestToJoinConfirmationEmailTemplateId")
        };

        IConfiguration mockConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _useCase = new CreateOrganisationJoinRequestUseCase(
            _mockOrganisationRepository.Object,
            _mockPersonRepository.Object,
            _mockOrganisationJoinRequestRepository.Object,
            _mockMapper.Object,
            mockConfiguration,
            _notifyApiClient.Object,
            _logger.Object
        );
    }

    [Fact]
    public async Task Execute_WhenOrganisationIsUnknown_ShouldThrowUnknownOrganisationException()
    {
        var organisationId = Guid.NewGuid();
        var createJoinRequestCommand = new CreateOrganisationJoinRequest { PersonId = Guid.NewGuid() };

        _mockOrganisationRepository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync((OrganisationInformation.Persistence.Organisation)null!);

        Func<Task> action = async () => await _useCase.Execute((organisationId, createJoinRequestCommand));

        await action.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task Execute_WhenOrganisationIsknownAndPersonAlreadyInvited_ShouldThrowPersonAlreadyInvitedToOrganisationException()
    {
        var createJoinRequestCommand = new CreateOrganisationJoinRequest { PersonId = _person.Guid };
        
        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        _mockOrganisationJoinRequestRepository.Setup(repo => repo.FindByOrganisationAndPerson(_organisation.Guid, _person.Id))
            .ReturnsAsync(new OrganisationInformation.Persistence.OrganisationJoinRequest() { Guid = Guid.NewGuid(), Status = OrganisationJoinRequestStatus.Pending});

        Func<Task> action = async () => await _useCase.Execute((_organisation.Guid, createJoinRequestCommand));

        await action.Should().ThrowAsync<PersonAlreadyInvitedToOrganisationException>();
    }

    [Fact]
    public async Task Execute_WhenPersonIsUnknown_ShouldThrowUnknownPersonException()
    {
        var personId = Guid.NewGuid();
        var createJoinRequestCommand = new CreateOrganisationJoinRequest { PersonId = personId };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(personId))
            .ReturnsAsync((Person)null!);

        Func<Task> action = async () => await _useCase.Execute((_organisation.Guid, createJoinRequestCommand));

        await action.Should().ThrowAsync<UnknownPersonException>()
            .WithMessage($"Unknown person {personId}.");
    }

    [Fact]
    public async Task Execute_ShouldCreateAndSaveOrganisationJoinRequest()
    {
        var createJoinRequestCommand = new CreateOrganisationJoinRequest { PersonId = _person.Guid };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        _mockMapper.Setup(mapper => mapper.Map<OrganisationJoinRequest>(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()))
            .Returns(new OrganisationJoinRequest
            {
                Status = OrganisationJoinRequestStatus.Pending,
                Id = default,
                Person = null!,
                Organisation = null!,
                ReviewedBy = null!,
                ReviewedOn = default
            });

        var result = await _useCase.Execute((_organisation.Guid, createJoinRequestCommand));

        result.Should().NotBeNull();
        result.Status.Should().Be(OrganisationJoinRequestStatus.Pending);

        _mockOrganisationJoinRequestRepository.Verify(repo => repo.Save(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()), Times.Once);
        _mockMapper.Verify(mapper => mapper.Map<OrganisationJoinRequest>(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()), Times.Once);
    }

    [Fact]
    public async Task ItShouldSendUserRequestSentEmail()
    {
        var createJoinRequestCommand = new CreateOrganisationJoinRequest { PersonId = _person.Guid };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        _mockMapper.Setup(mapper => mapper.Map<OrganisationJoinRequest>(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()))
            .Returns(new OrganisationJoinRequest
            {
                Status = OrganisationJoinRequestStatus.Pending,
                Id = default,
                Person = null!,
                Organisation = null!,
                ReviewedBy = null!,
                ReviewedOn = default
            });

        await _useCase.Execute((_organisation.Guid, createJoinRequestCommand));

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == _person.Email &&
            req.TemplateId == "RequestToJoinConfirmationEmailTemplateId" &&
            req.Personalisation!["org_name"] == _organisation.Name
        )), Times.Once);
    }

    [Fact]
    public async Task ItShouldSendEmailsToOrgAdmins()
    {
        var createJoinRequestCommand = new CreateOrganisationJoinRequest { PersonId = _person.Guid };

        _mockOrganisationRepository.Setup(repo => repo.Find(_organisation.Guid))
            .ReturnsAsync(_organisation);

        _mockPersonRepository.Setup(repo => repo.Find(_person.Guid))
            .ReturnsAsync(_person);

        var adminUsers = new List<Person>
        {
            new Person
            {
                Email = "admin1@example.com",
                FirstName = "Admin",
                LastName = "One",
                Guid = new Guid()
            },
            new Person
            {
                Email = "admin2@example.com",
                FirstName = "Admin",
                LastName = "Two",
                Guid = new Guid()
            }
        };

        _mockOrganisationRepository.Setup(repo => repo.FindOrganisationPersons(_organisation.Guid))
            .ReturnsAsync(adminUsers.Select(admin => new OrganisationPerson
            {
                Person = admin,
                Scopes = ["ADMIN"],
                Organisation = null!
            }).ToList());

        _mockMapper.Setup(mapper => mapper.Map<OrganisationJoinRequest>(It.IsAny<CO.CDP.OrganisationInformation.Persistence.OrganisationJoinRequest>()))
            .Returns(new OrganisationJoinRequest
            {
                Status = OrganisationJoinRequestStatus.Pending,
                Id = default,
                Person = null!,
                Organisation = null!,
                ReviewedBy = null!,
                ReviewedOn = default
            });

        await _useCase.Execute((_organisation.Guid, createJoinRequestCommand));

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == "admin1@example.com" &&
            req.TemplateId == "RequestToJoinNotifyOrgAdminTemplateId" &&
            req.Personalisation!["org_name"] == _organisation.Name &&
            req.Personalisation["first_name"] == "Admin" &&
            req.Personalisation["last_name"] == "One" &&
            req.Personalisation["requester_first_name"] == _person.FirstName &&
            req.Personalisation["requester_last_name"] == _person.LastName
        )), Times.Once);

        _notifyApiClient.Verify(x => x.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == "admin2@example.com" &&
            req.TemplateId == "RequestToJoinNotifyOrgAdminTemplateId" &&
            req.Personalisation!["org_name"] == _organisation.Name &&
            req.Personalisation["first_name"] == "Admin" &&
            req.Personalisation["last_name"] == "Two" &&
            req.Personalisation["requester_first_name"] == _person.FirstName &&
            req.Personalisation["requester_last_name"] == _person.LastName
        )), Times.Once);
    }
}
