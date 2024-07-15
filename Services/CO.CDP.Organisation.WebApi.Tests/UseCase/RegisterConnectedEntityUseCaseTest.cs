using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using System;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class RegisterConnectedEntityUseCaseTest(AutoMapperFixture mapperFixture) : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepo = new();
    private readonly Mock<Persistence.IPersonRepository> _persons = new();
    private readonly Mock<Persistence.IConnectedEntityRepository> _connectedEntityRepo = new();
    private readonly Guid _generatedGuid = Guid.NewGuid();
    private RegisterConnectedEntityUseCase UseCase => new(_connectedEntityRepo.Object, _organisationRepo.Object, mapperFixture.Mapper, () => _generatedGuid);

    [Fact]
    public async Task Execute_ShouldThrowUnknownOrganisationException_WhenOrganisationNotFound()
    {
        var organisationId = Guid.NewGuid();
        Persistence.Organisation? organisation = null;
        var registerConnectedEntity = GivenRegisterConnectedEntity(organisationId);

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        Func<Task> act = async () => await UseCase.Execute((organisationId, registerConnectedEntity));

        await act.Should()
            .ThrowAsync<UnknownOrganisationException>()
            .WithMessage($"Unknown organisation {organisationId}.");
    }

    [Fact]
    public async Task ItReturnsTheRegisteredConnectedEntity()
    {
        var organisationId = Guid.NewGuid();

        var command = (organisationId, GivenRegisterConnectedEntity(organisationId));

        var org = GivenOrganisationExist(organisationId);

        _organisationRepo.Setup(x => x.Save(org));

        var result = await UseCase.Execute(command);

        var expectedConnectedEntity = GivenRegisterConnectedEntity(organisationId);

        _connectedEntityRepo.Verify(repo => repo.Save(It.IsAny<Persistence.ConnectedEntity>()), Times.Once);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ItSavesNewConnectedEntityInTheRepository()
    {
        var organisationId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        var registerConnectedEntity = GivenRegisterConnectedEntity(organisationId);

        var command = (organisationId, registerConnectedEntity);

        Persistence.ConnectedEntity? persistanceConnectedEntity = null;

        var ce = GivenConnectedEntityExists(organisationId, entityId);

        _connectedEntityRepo
            .Setup(x => x.Save(It.IsAny<Persistence.ConnectedEntity>()))
            .Callback<Persistence.ConnectedEntity>(b => persistanceConnectedEntity = b);

        var result = await UseCase.Execute(command);

        _connectedEntityRepo.Verify(e => e.Save(It.IsAny<Persistence.ConnectedEntity>()), Times.Once);

        persistanceConnectedEntity.Should().NotBeNull();
        persistanceConnectedEntity.As<Persistence.ConnectedEntity>().Organisation!.OrganisationId.Should().Be(organisationId);
        persistanceConnectedEntity.As<Persistence.ConnectedEntity>().CompanyHouseNumber.Should().Be("CH_1");
    }

    private Persistence.Organisation GivenOrganisationExist(Guid organisationId)
    {
        var org = new Persistence.Organisation
        {
            Name = "TheOrganisation",
            Guid = organisationId,
            Addresses = [new Persistence.Organisation.OrganisationAddress
                {
                    Type = AddressType.Registered,
                    Address = new Persistence.Address
                    {
                        StreetAddress = "1234 Example St",
                        StreetAddress2 = "",
                        Locality = "Example City",
                        Region = "Test Region",
                        PostalCode = "12345",
                        CountryName = "Exampleland"
                    }
                }],
            Tenant = It.IsAny<Tenant>()
        };

        _organisationRepo.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(org);

        return org;
    }

    private RegisterConnectedEntity GivenRegisterConnectedEntity(Guid organisationId)
    {
        return new RegisterConnectedEntity
        {
            EntityType = ConnectedEntityType.Organisation,
            Organisation = new ConnectedOrganisation
            {
                Category = ConnectedOrganisationCategory.RegisteredCompany,
                Name = "Org1",
                OrganisationId = organisationId
            },
            RegisterName = "ABC",
            RegisteredDate = DateTime.Now,
            CompanyHouseNumber = "CH_1"
        };
    }

    private Persistence.ConnectedEntity GivenConnectedEntityExists(Guid guid, Guid entityId)
    {
        Persistence.ConnectedEntity entity = new Persistence.ConnectedEntity
        {
            Guid = entityId,
            EntityType = Persistence.ConnectedEntity.ConnectedEntityType.Organisation,
            SupplierOrganisation = GivenOrganisationExist(guid)
        };

        _connectedEntityRepo.Setup(repo => repo.Find(guid, entityId))
            .ReturnsAsync(entity);

        return entity;
    }
}