using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class UpdateBuyerInformationUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly UpdateBuyerInformationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();

    public UpdateBuyerInformationUseCaseTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new UpdateBuyerInformationUseCase(_organisationRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_ShouldUpdateBuyerInformation_WhenOrganisationExists()
    {
        var updateBuyerInformation = GetUpdateBuyerInformation();

        var command = (_organisationId, GetUpdateBuyerInformation());

        var organisation = GetOrganisation();

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId)).ReturnsAsync(organisation);

        var result = await _useCase.Execute(command);

        result.Should().BeTrue();
        organisation?.BuyerInfo?.BuyerType.Should().Be("NewType");
        _organisationRepositoryMock.Verify(repo => repo.Save(organisation!), Times.Once);

    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        OrganisationInformation.Persistence.Organisation? organisation = null;
        var updateBuyerInformation = GetUpdateBuyerInformation();

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateBuyerInformation));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_ShouldThrowBuyerInfoNotExistException_WhenBuyerInfoIsNull()
    {
        var updateBuyerInformation = GetUpdateBuyerInformation();
        var organisation = GetOrganisation();
        organisation.BuyerInfo = null;

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateBuyerInformation));

        await act.Should()
            .ThrowAsync<BuyerInfoNotExistException>()
            .WithMessage($"Buyer information for organisation {_organisationId} not exist.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateBuyerInformationCommand_WhenUpdateTypeIsUnknown()
    {
        var updateBuyerInformation = new UpdateBuyerInformation
        {
            Type = (BuyerInformationUpdateType)999, // Unknown type
            BuyerInformation = new Model.BuyerInformation
            {
                BuyerType = "NewType",
                DevolvedRegulations = [DevolvedRegulation.NorthernIreland]
            }
        };
        var organisation = GetOrganisation();

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateBuyerInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateBuyerInformationCommand>()
            .WithMessage("Unknown buyer information update type.");
    }

    [Fact]
    public async Task Execute_ShouldThrowInvalidUpdateBuyerInformationCommand_WhenBuyerTypeIsNull()
    {
        var updateBuyerInformation = new UpdateBuyerInformation
        {
            Type = BuyerInformationUpdateType.BuyerOrganisationType,
            BuyerInformation = new Model.BuyerInformation
            {
                BuyerType = null,
                DevolvedRegulations = [DevolvedRegulation.NorthernIreland]
            }
        };
        var organisation = GetOrganisation();

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateBuyerInformation));

        await act.Should()
            .ThrowAsync<InvalidUpdateBuyerInformationCommand>()
            .WithMessage("Missing buyer type.");
    }

    private UpdateBuyerInformation GetUpdateBuyerInformation()
    {
        return new UpdateBuyerInformation
        {
            Type = BuyerInformationUpdateType.BuyerOrganisationType,
            BuyerInformation = new Model.BuyerInformation
            {
                BuyerType = "NewType",
                DevolvedRegulations = [DevolvedRegulation.NorthernIreland]
            }
        };
    }

    private OrganisationInformation.Persistence.Organisation GetOrganisation()
    {
        return new OrganisationInformation.Persistence.Organisation
        {
            ContactPoints = [new OrganisationInformation.Persistence.ContactPoint { Email = "test@test.com" }],
            Guid = _organisationId,
            Name = "Test",
            Type = OrganisationType.Organisation,
            Tenant = It.IsAny<Tenant>(),
            BuyerInfo = new OrganisationInformation.Persistence.BuyerInformation
            { BuyerType = "NewType", DevolvedRegulations = [DevolvedRegulation.NorthernIreland] },
        };
    }
}