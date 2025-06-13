using AutoMapper;
using Moq;
using FluentAssertions;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.Authentication;
using CO.CDP.OrganisationInformation.Persistence.NonEfEntities;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class GetOrganisationsUseCaseTests
{
    private readonly Mock<IOrganisationRepository> _organisationRepositoryMock;
    private readonly GetOrganisationsUseCase _useCase;

    public GetOrganisationsUseCaseTests()
    {
        _organisationRepositoryMock = new Mock<IOrganisationRepository>();
        _useCase = new GetOrganisationsUseCase(_organisationRepositoryMock.Object);
    }

    [Fact]
    public async Task Execute_WithValidCommand_ReturnsMappedOrganisations_WithAdminPerson()
    {
        var command = new PaginatedOrganisationQuery(limit: 10, skip: 0, "buyer", "buyer");

        var orgName = "Organisation 1";
        var orgType = OrganisationType.Organisation;
        var organisationGuid = Guid.NewGuid();
        var approvedOn = new DateTimeOffset();
        var adminEmail = "admin@email.com";
        var identifierString = "GB-PPON:0001d4b3-e511-4382-9be4-36c1bb5a3411";
        var orgEmail = "organisation@email.com";

        var organisations = new Tuple<IList<OrganisationRawDto>, int>(
            new List<OrganisationRawDto>()
                {
                    new OrganisationRawDto
                    {
                        Id = 1,
                        Guid = organisationGuid,
                        Name = orgName,
                        Type = orgType,
                        Identifiers = identifierString,
                        ContactPoints = orgEmail,
                        Roles = [],
                        PendingRoles = [],
                        ApprovedOn = approvedOn,
                        AdminEmail = adminEmail,
                    }
                },
            1);

        List<OrganisationDto> OrganisationDtos =
        [
            new OrganisationDto
            {
                Id = organisationGuid,
                Name = orgName,
                Type = orgType,
                ContactPoints = [orgEmail],
                ApprovedOn = approvedOn,
                Roles = new List<PartyRole>(),
                PendingRoles = new List<PartyRole>(),
                AdminEmail = adminEmail,
                Identifiers = [identifierString]
            }
        ];

        _organisationRepositoryMock
            .Setup(repo => repo.GetPaginated(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip))
            .ReturnsAsync(organisations);

        var result = await _useCase.Execute(command);

        result.Item1.Should().BeEquivalentTo(OrganisationDtos);
        _organisationRepositoryMock.Verify(repo => repo.GetPaginated(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip), Times.Once);
    }


    [Fact]
    public async Task Execute_WhenNoOrganisationsExist_ReturnsEmptyList()
    {
        var command = new PaginatedOrganisationQuery(limit: 10, skip: 0, "buyer", "buyer");
        var organisations = new Tuple<IList<OrganisationRawDto>, int>([], 0);

        _organisationRepositoryMock
            .Setup(repo => repo.GetPaginated(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip))
            .ReturnsAsync(organisations);

        var result = await _useCase.Execute(command);

        result.Item1.Should().BeEmpty();
        _organisationRepositoryMock.Verify(repo => repo.GetPaginated(command.Role, command.PendingRole, command.SearchText, command.Limit, command.Skip), Times.Once);
    }
}
