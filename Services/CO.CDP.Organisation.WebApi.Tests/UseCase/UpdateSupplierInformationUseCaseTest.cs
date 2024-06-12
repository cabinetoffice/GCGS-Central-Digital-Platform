using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateSupplierInformationUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly UpdateSupplierInformationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();

    public UpdateSupplierInformationUseCaseTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new UpdateSupplierInformationUseCase(_organisationRepositoryMock.Object);
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
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenVatInfoIsMissing()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.Vat,
            SupplierInformation = new SupplierInfo()
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Missing vat identifier.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateSupplierInformationCommand_WhenVatNumberIsMissing()
    {
        var updateSupplierInformation = new UpdateSupplierInformation
        {
            Type = SupplierInformationUpdateType.Vat,
            SupplierInformation = new SupplierInfo { HasVatNumber = true }
        };
        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(Organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateSupplierInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Missing vat identifier.");
    }

    private Persistence.Organisation Organisation =>
        new Persistence.Organisation
        {
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
            ContactPoint = new Persistence.Organisation.OrganisationContactPoint { Email = "test@test.com" },
            SupplierInfo = new Persistence.Organisation.SupplierInformation()
        };
}