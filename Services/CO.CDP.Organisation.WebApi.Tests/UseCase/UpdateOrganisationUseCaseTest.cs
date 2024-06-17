using AutoMapper;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateOrganisationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly Mock<IMapper> _mapper;
    private readonly UpdateOrganisationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();

    public UpdateOrganisationUseCaseTest()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _mapper = new Mock<IMapper>();
        _useCase = new UpdateOrganisationUseCase(_organisationRepositoryMock.Object, _mapper.Object);
    }

    [Fact]
    public async Task Execute_ShouldUpdateOrganisation_WhenOrganisationExists()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new OrganisationInfo
            {
                AdditionalIdentifiers = [new OrganisationIdentifier {
                    Id = "FakeId",
                    LegalName = "Illigal",
                    Scheme = "FakeScheme"
                }]
            }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, updateOrganisation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenUpdateTypeIsUnknown()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = (OrganisationUpdateType)999, // Unknown type
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Unknown organisation update type.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenAdditionalIdentifiersIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.AdditionalIdentifiers,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing additional identifiers.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateOrganisationCommand_WhenAddressIsNull()
    {
        var updateOrganisation = new UpdateOrganisation
        {
            Type = OrganisationUpdateType.Address,
            Organisation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateOrganisation));

        await act.Should()
            .ThrowAsync<InvalidUpdateOrganisationCommand>()
            .WithMessage("Missing organisation address.");
    }

    private Persistence.Organisation Organisation =>
        new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }]
        };
}