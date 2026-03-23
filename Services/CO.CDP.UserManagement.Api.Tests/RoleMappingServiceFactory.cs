using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Data;
using CO.CDP.UserManagement.Infrastructure.Repositories;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Api.Tests;

internal static class RoleMappingServiceFactory
{
    internal static (RoleMappingService Service, UserManagementDbContext UserManagementContext, Mock<IOrganisationApiAdapter> OrganisationApiAdapter) Create(
        Action<Mock<IOrganisationApiAdapter>>? configureAdapter = null)
    {
        var userManagementContext = CreateUserManagementContext();
        var adapterMock = CreateDefaultOrganisationApiAdapterMock();
        configureAdapter?.Invoke(adapterMock);
        SeedDefaultRoleDefinitions(userManagementContext);

        var service = BuildService(userManagementContext, adapterMock.Object);
        return (service, userManagementContext, adapterMock);
    }

    internal static RoleMappingService BuildService(UserManagementDbContext context, IOrganisationApiAdapter organisationApiAdapter) =>
        new(
            new UserOrganisationMembershipRepository(context),
            new UserApplicationAssignmentRepository(context),
            new OrganisationRepository(context),
            new RoleRepository(context),
            organisationApiAdapter);

    internal static Mock<IOrganisationApiAdapter> CreateDefaultOrganisationApiAdapterMock()
    {
        var mock = new Mock<IOrganisationApiAdapter>();
        mock.Setup(c => c.GetPartyRolesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ISet<CorePartyRole>)new HashSet<CorePartyRole>());
        return mock;
    }

    internal static void SeedDefaultRoleDefinitions(UserManagementDbContext context)
    {
        if (context.OrganisationRoles.Any())
        {
            return;
        }

        context.OrganisationRoles.AddRange(
            CreateRoleDefinition(OrganisationRole.Agent,  syncToOrganisationInformation: true,  autoAssignDefaultApplications: false, organisationInformationScopes: []),
            CreateRoleDefinition(OrganisationRole.Member, syncToOrganisationInformation: true,  autoAssignDefaultApplications: true,  organisationInformationScopes: ["VIEWER"]),
            CreateRoleDefinition(OrganisationRole.Admin,  syncToOrganisationInformation: true,  autoAssignDefaultApplications: true,  organisationInformationScopes: ["ADMIN"]),
            CreateRoleDefinition(OrganisationRole.Owner,  syncToOrganisationInformation: true,  autoAssignDefaultApplications: true,  organisationInformationScopes: ["ADMIN"]));

        context.SaveChanges();
    }

    private static UserManagementDbContext CreateUserManagementContext()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<UserManagementDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new UserManagementDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static OrganisationRoleEntity CreateRoleDefinition(
        OrganisationRole role,
        bool syncToOrganisationInformation,
        bool autoAssignDefaultApplications,
        IEnumerable<string> organisationInformationScopes) =>
        new()
        {
            Id = (int)role,
            DisplayName = role.ToString(),
            SyncToOrganisationInformation = syncToOrganisationInformation,
            AutoAssignDefaultApplications = autoAssignDefaultApplications,
            OrganisationInformationScopes = organisationInformationScopes.ToList(),
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = "test"
        };
}
