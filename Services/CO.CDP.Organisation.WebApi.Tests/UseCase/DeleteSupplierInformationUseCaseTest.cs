using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;
public class DeleteSupplierInformationUseCaseTest
{
    private readonly Mock<IOrganisationRepository> _organisationRepository;
    private readonly DeleteSupplierInformationUseCase _useCase;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _tradeAssuranceId = Guid.NewGuid();

    public DeleteSupplierInformationUseCaseTest()
    {
        _organisationRepository = new Mock<IOrganisationRepository>();
        _useCase = new DeleteSupplierInformationUseCase(_organisationRepository.Object);
    }

    [Fact]
    public async Task Execute_ValidTradeAssurance_RemovesTradeAssurance_ReturnsTrue()
    {
        var organisation = Organisation();
        SetupOrganisationRepository(organisation);
        var deleteSupplierInformation = new DeleteSupplierInformation
        {
            Type = SupplierInformationDeleteType.TradeAssurance,
            TradeAssuranceId = _tradeAssuranceId
        };

        var result = await _useCase.Execute((_organisationId, deleteSupplierInformation));

        result.Should().BeTrue();
        organisation.SupplierInfo!.TradeAssurances.Should().BeEmpty();
        _organisationRepository.Verify(repo => repo.Save(organisation), Times.Once);
    }

    [Fact]
    public async Task Execute_UnknownOrganisation_ThrowsUnknownOrganisationException()
    {
        SetupOrganisationRepository(null);

        Func<Task> act = async () => await _useCase.Execute((_organisationId, It.IsAny<DeleteSupplierInformation>()));

        await act.Should().ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {_organisationId}.");
    }

    [Fact]
    public async Task Execute_NoSupplierInfo_ThrowsSupplierInfoNotExistException()
    {
        var organisation = Organisation(withoutSupplierInfo: true);
        SetupOrganisationRepository(organisation);
        var deleteSupplierInformation = new DeleteSupplierInformation
        {
            Type = SupplierInformationDeleteType.TradeAssurance,
            TradeAssuranceId = _tradeAssuranceId
        };

        Func<Task> act = async () => await _useCase.Execute((_organisationId, deleteSupplierInformation));

        await act.Should().ThrowAsync<SupplierInfoNotExistException>()
            .WithMessage($"Supplier information for organisation {_organisationId} not exist.");
    }

    [Fact]
    public async Task Execute_MissingTradeAssuranceId_ThrowsInvalidUpdateSupplierInformationCommand()
    {
        var organisation = Organisation(withoutTradeAssurance: true);
        SetupOrganisationRepository(organisation);
        var deleteSupplierInformation = new DeleteSupplierInformation
        {
            Type = SupplierInformationDeleteType.TradeAssurance,
            TradeAssuranceId = null
        };

        Func<Task> act = async () => await _useCase.Execute((_organisationId, deleteSupplierInformation));

        await act.Should().ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Missing trade assurance id.");
    }

    [Fact]
    public async Task Execute_UnknownDeleteType_ThrowsInvalidUpdateSupplierInformationCommand()
    {
        var organisation = Organisation();
        SetupOrganisationRepository(organisation);
        var deleteSupplierInformation = new DeleteSupplierInformation
        {
            Type = (SupplierInformationDeleteType)999, // Invalid type
            TradeAssuranceId = _tradeAssuranceId
        };

        Func<Task> act = async () => await _useCase.Execute((_organisationId, deleteSupplierInformation));

        await act.Should().ThrowAsync<InvalidUpdateSupplierInformationCommand>()
            .WithMessage("Unknown supplier information delete type.");
    }

    private Persistence.Organisation Organisation(bool withoutSupplierInfo = false, bool withoutTradeAssurance = false)
    {
        var org = new Persistence.Organisation { Guid = _organisationId, Name = "Test", Tenant = It.IsAny<Tenant>() };
        if (!withoutSupplierInfo)
        {
            org.SupplierInfo = new Persistence.Organisation.SupplierInformation();

            if (!withoutTradeAssurance)
            {
                org.SupplierInfo.TradeAssurances = [new Persistence.Organisation.TradeAssurance {
                    Guid= _tradeAssuranceId,
                    AwardedByPersonOrBodyName = "Award Body",
                    ReferenceNumber = "Ref123456",
                    DateAwarded = DateTime.Now.AddDays(1),
                }];
            }
        }
        return org;
    }

    private void SetupOrganisationRepository(Persistence.Organisation? organisation)
        => _organisationRepository.Setup(repo => repo.Find(_organisationId))
            .ReturnsAsync(organisation);
}