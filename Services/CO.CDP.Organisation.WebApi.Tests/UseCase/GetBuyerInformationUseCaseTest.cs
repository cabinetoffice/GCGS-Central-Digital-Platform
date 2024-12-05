using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.Tests.AutoMapper;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.OrganisationInformation.Persistence;
using FluentAssertions;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetBuyerInformationUseCaseTest(AutoMapperFixture mapperFixture)
    : IClassFixture<AutoMapperFixture>
{
    private readonly Mock<IOrganisationRepository> _repository = new();
    private GetBuyerInformationUseCase UseCase => new(_repository.Object, mapperFixture.Mapper);

    [Fact]
    public async Task Execute_ValidOrganisationWithBuyerInfo_ReturnsBuyerInformation()
    {
        var organisationId = Guid.NewGuid();
        var organisation = FakeOrganisation();

        _repository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(organisation);

        var result = await UseCase.Execute(organisationId);

        result.Should().NotBeNull();
        result.As<BuyerInformation>().BuyerType.Should().Be("FakeBuyerType");
        result.As<BuyerInformation>().DevolvedRegulations.Should().NotBeNullOrEmpty();
        result.As<BuyerInformation>().DevolvedRegulations.Should().Contain(DevolvedRegulation.NorthernIreland);
    }

    [Fact]
    public async Task Execute_OrganisationNotFound_ReturnsNull()
    {
        var organisationId = Guid.NewGuid();

        _repository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync((Persistence.Organisation?)null);

        var result = await UseCase.Execute(organisationId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Execute_OrganisationWithoutBuyerInfo_ReturnsNull()
    {
        var organisationId = Guid.NewGuid();

        _repository.Setup(repo => repo.Find(organisationId))
            .ReturnsAsync(FakeOrganisation(false));

        var result = await UseCase.Execute(organisationId);

        result.Should().BeNull();
    }

    private static Persistence.Organisation FakeOrganisation(bool? withBuyerInfo = true)
    {
        Persistence.Organisation org = new()
        {
            Guid = Guid.NewGuid(),
            Name = "FakeOrg",
            Tenant = new Tenant
            {
                Guid = Guid.NewGuid(),
                Name = "Tenant 101"
            },
            ContactPoints = [new Persistence.Organisation.ContactPoint { Email = "contact@test.org" }]
        };

        if (withBuyerInfo == true)
        {
            var devolvedRegulations = new List<DevolvedRegulation>();
            devolvedRegulations.Add(DevolvedRegulation.NorthernIreland);

            org.BuyerInfo = new Persistence.Organisation.BuyerInformation
            {
                BuyerType = "FakeBuyerType",
                DevolvedRegulations = devolvedRegulations,
            };
        }

        return org;
    }
}