using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateSupplierInformationUseCaseTests : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly UpdateSupplierInformationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();

    public UpdateSupplierInformationUseCaseTests(AutoMapperFixture mapperFixture)
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new UpdateSupplierInformationUseCase(_organisationRepositoryMock.Object, mapperFixture.Mapper);
    }

    [Fact]
    public async Task Execute_ShouldUpdateSupplierInformation_WhenOrganisationExists()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new SupplierInfo { SupplierType = SupplierType.Individual }
        };
        var organisation = Organisation;
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute((_organisationId, updateSupplierInformation));

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync((Persistence.Organisation?)null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowSupplierInfoNotExistException_WhenSupplierInfoIsNull()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new()
        };
        var organisation = Organisation;
        organisation.SupplierInfo = null;

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<SupplierInfoNotExistException>()
            .WithMessage($"Supplier information for organisation {_organisationId} not exist.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenUpdateTypeIsUnknown()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = (SupplierInformationUpdateType)999, // Unknown type
            SupplierInformation = new()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Unknown supplier information update type.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenSupplierTypeIsNull()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new SupplierInfo()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Missing supplier type.");
    }

    [Fact]
    public async Task Execute_ShouldUpdateSupplierType_WhenSupplierTypeIsProvided()
    {
        var organisation = Organisation;
        var updateInfo = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new SupplierInfo { SupplierType = SupplierType.Individual }
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var command = (_organisationId, updateInfo);

        var result = await _useCase.Execute(command);

        organisation.Should().NotBeNull();
        organisation.SupplierInfo.Should().NotBeNull();
        organisation.SupplierInfo!.SupplierType.Should().NotBeNull();
        organisation.SupplierInfo.SupplierType.Should().Be(SupplierType.Individual);
        organisation.SupplierInfo.OperationTypes.Should().BeEmpty();
        organisation.SupplierInfo.LegalForm.Should().BeNull();
        organisation.SupplierInfo.CompletedLegalForm.Should().BeFalse();
        organisation.SupplierInfo.CompletedOperationType.Should().BeFalse();

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldResetFields_WhenSupplierTypeIsNotIndividual()
    {
        var organisation = new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.Organisation.SupplierInformation
            {
                SupplierType = SupplierType.Individual,  // Initially set to Individual
                OperationTypes = new List<OperationType> { OperationType.SmallOrMediumSized },
                LegalForm = new Persistence.Organisation.LegalForm
                {
                    RegisteredUnderAct2006 = true,
                    RegisteredLegalForm = "Private Limited",
                    LawRegistered = "UK",
                    RegistrationDate = DateTimeOffset.UtcNow.AddYears(-10)
                },
                CompletedLegalForm = true,
                CompletedOperationType = true
            }
        };     

        var updateInfo = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.SupplierType,
            SupplierInformation = new SupplierInfo { SupplierType = SupplierType.Organisation }
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var command = (_organisationId, updateInfo);
        var result = await _useCase.Execute(command);

        
        organisation.SupplierInfo.Should().NotBeNull();
        organisation.SupplierInfo.SupplierType.Should().Be(SupplierType.Organisation);
        
        organisation.SupplierInfo.OperationTypes.Should().NotBeNull();
        organisation.SupplierInfo.LegalForm.Should().NotBeNull();
        organisation.SupplierInfo.CompletedLegalForm.Should().BeTrue();
        organisation.SupplierInfo.CompletedOperationType.Should().BeTrue();

        result.Should().BeTrue();
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenLegalFormIsNull()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.LegalForm,
            SupplierInformation = new SupplierInfo()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Missing legal form.");
    }

    private Persistence.Organisation Organisation =>
        new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "test@test.com" }],
            SupplierInfo = new Persistence.Organisation.SupplierInformation()
        };
}