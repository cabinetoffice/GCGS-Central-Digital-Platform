using System;
using System.Threading.Tasks;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using FluentAssertions;
using Moq;
using Xunit;
using CO.CDP.OrganisationInformation;
using static CO.CDP.Organisation.WebApi.UseCase.UpdateBuyerInformationUseCase;

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

    private (Guid organisationId, UpdateBuyerInformation updateBuyerInformation) CreateCommand(
        Guid organisationId,
        BuyerInformationUpdateType updateType,
        BuyerInformation? buyerInformation = null)
    {
        return (organisationId, new UpdateBuyerInformation
        {
            Type = updateType,
            BuyerInformation = buyerInformation ?? new BuyerInformation { DevolvedRegulations = [DevolvedRegulation.NorthernIreland] }
        });
    }

    [Fact]
    public async Task Execute_ShouldUpdateBuyerInformation_WhenOrganisationExists()
    {
        var command = CreateCommand(
                            _organisationId,
                            BuyerInformationUpdateType.BuyerOrganisationType,
                            new BuyerInformation {
                                BuyerType = "NewType",
                                DevolvedRegulations = [DevolvedRegulation.NorthernIreland]
                            });

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
        CO.CDP.OrganisationInformation.Persistence.Organisation? organisation = null;
        var updateBuyerInformation = new UpdateBuyerInformation
        {
            Type = BuyerInformationUpdateType.BuyerOrganisationType,
            BuyerInformation = new BuyerInformation
            {
                BuyerType = "NewType",
                DevolvedRegulations = [DevolvedRegulation.NorthernIreland]
            }
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateBuyerInformation));

        await act.Should().ThrowAsync<UpdateBuyerInformationUseCase.UpdateBuyerInformationException.UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }


    [Fact]
    public async Task Execute_ShouldThrowBuyerInfoNotExistException_WhenBuyerInfoIsNull()
    {        
        var updateBuyerInformation = new UpdateBuyerInformation
        {
            Type = BuyerInformationUpdateType.BuyerOrganisationType,
            BuyerInformation = new BuyerInformation
            {
                BuyerType = "NewType",
                DevolvedRegulations = [DevolvedRegulation.NorthernIreland]
            }
        };


        var organisation = new CO.CDP.OrganisationInformation.Persistence.Organisation
        {
            ContactPoint = new CO.CDP.OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            { Email = "test@test.com" },
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
        };

        _organisationRepositoryMock.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);

        // Act
        Func<Task> act = async () => await _useCase.Execute((_organisationId, updateBuyerInformation));

        // Assert
        await act.Should()
            .ThrowAsync<UpdateBuyerInformationUseCase.UpdateBuyerInformationException.BuyerInfoNotExistException>()
            .WithMessage($"Buyer information for organisation {_organisationId} not exist.");
    }

    [Fact]
    public async Task Execute_ShouldThrowUnknownBuyerInformationUpdateTypeException_WhenUpdateTypeIsUnknown()
    {
        var updateBuyerInformation = new UpdateBuyerInformation
        {
            Type = (BuyerInformationUpdateType)999, // Unknown type
            BuyerInformation = new BuyerInformation
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
            .ThrowAsync<UpdateBuyerInformationException.UnknownBuyerInformationUpdateTypeException>()
            .WithMessage("Unknown buyer information update type.");
    }

    private CO.CDP.OrganisationInformation.Persistence.Organisation GetOrganisation()
    {
        return  new CO.CDP.OrganisationInformation.Persistence.Organisation
        {
            ContactPoint = new CO.CDP.OrganisationInformation.Persistence.Organisation.OrganisationContactPoint
            { Email = "test@test.com" },
            Guid = _organisationId,
            Name = "Test",
            Tenant = It.IsAny<Tenant>(),
            BuyerInfo = new CO.CDP.OrganisationInformation.Persistence.Organisation.BuyerInformation
            { BuyerType = "NewType", DevolvedRegulations = [DevolvedRegulation.NorthernIreland] },
        };
    }
}
