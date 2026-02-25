using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;
using FluentAssertions;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Api.Tests.Models;

public class MappingExtensionsTests
{
    [Fact]
    public void ApplicationMappingExtensions_ToResponse_MapsFields()
    {
        var createdAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var application = new Application
        {
            Id = 1,
            Name = "App",
            ClientId = "client",
            Description = "desc",
            Category = "cat",
            IsActive = true,
            CreatedAt = createdAt
        };

        var response = application.ToResponse();

        response.Id.Should().Be(1);
        response.ClientId.Should().Be("client");
        response.Category.Should().Be("cat");
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void ApplicationMappingExtensions_ToSummaryResponse_MapsFields()
    {
        var application = new Application
        {
            Id = 2,
            Name = "App",
            ClientId = "client",
            IsActive = false
        };

        var response = application.ToSummaryResponse();

        response.Id.Should().Be(2);
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public void OrganisationMappingExtensions_ToResponse_MapsFields()
    {
        var createdAt = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var organisation = new CoreOrganisation
        {
            Id = 3,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org",
            Slug = "org",
            IsActive = true,
            CreatedAt = createdAt
        };

        var response = organisation.ToResponse();

        response.Id.Should().Be(3);
        response.Slug.Should().Be("org");
        response.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void OrganisationMappingExtensions_ToSummaryResponse_MapsFields()
    {
        var organisation = new CoreOrganisation
        {
            Id = 4,
            CdpOrganisationGuid = Guid.NewGuid(),
            Name = "Org",
            Slug = "org",
            IsActive = false
        };

        var response = organisation.ToSummaryResponse();

        response.Id.Should().Be(4);
        response.IsActive.Should().BeFalse();
    }

    [Fact]
    public void PermissionMappingExtensions_ToResponses_MapsFields()
    {
        var permission = new ApplicationPermission
        {
            Id = 5,
            ApplicationId = 6,
            Name = "Read",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };

        var response = new[] { permission }.ToResponses().Single();

        response.Id.Should().Be(5);
        response.ApplicationId.Should().Be(6);
        response.Name.Should().Be("Read");
    }

    [Fact]
    public void RoleMappingExtensions_ToResponse_IncludesPermissionsByDefault()
    {
        var role = new ApplicationRole
        {
            Id = 7,
            ApplicationId = 8,
            Name = "Admin",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system",
            Permissions = new List<ApplicationPermission>
            {
                new()
                {
                    Id = 1,
                    ApplicationId = 8,
                    Name = "Read",
                    IsActive = true,
                    CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
                    CreatedBy = "system"
                }
            }
        };

        var response = role.ToResponse();

        response.Permissions.Should().NotBeNull();
        response.Permissions!.Should().ContainSingle();
    }

    [Fact]
    public void RoleMappingExtensions_ToResponse_ExcludePermissions_ReturnsNull()
    {
        var role = new ApplicationRole
        {
            Id = 9,
            ApplicationId = 8,
            Name = "Admin",
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 4, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };

        var response = role.ToResponse(includePermissions: false);

        response.Permissions.Should().BeNull();
    }

    [Fact]
    public void OrganisationApplicationMappingExtensions_ToResponse_IncludeDetails()
    {
        var orgApp = new OrganisationApplication
        {
            Id = 10,
            OrganisationId = 11,
            ApplicationId = 12,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system",
            Organisation = new CoreOrganisation
            {
                Id = 11,
                CdpOrganisationGuid = Guid.NewGuid(),
                Name = "Org",
                Slug = "org",
                IsActive = true
            },
            Application = new Application
            {
                Id = 12,
                Name = "App",
                ClientId = "client",
                IsActive = true
            }
        };

        var response = orgApp.ToResponse();

        response.Organisation.Should().NotBeNull();
        response.Application.Should().NotBeNull();
    }

    [Fact]
    public void OrganisationApplicationMappingExtensions_ToResponse_ExcludeDetails()
    {
        var orgApp = new OrganisationApplication
        {
            Id = 10,
            OrganisationId = 11,
            ApplicationId = 12,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 5, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system"
        };

        var response = orgApp.ToResponse(includeDetails: false);

        response.Organisation.Should().BeNull();
        response.Application.Should().BeNull();
    }

    [Fact]
    public void OrganisationUserMappingExtensions_ToResponse_UsesPersonDetails()
    {
        var personId = Guid.NewGuid();
        var membership = new UserOrganisationMembership
        {
            Id = 13,
            OrganisationId = 11,
            UserPrincipalId = "user-1",
            CdpPersonId = personId,
            OrganisationRole = OrganisationRole.Admin,
            IsActive = true,
            JoinedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        };
        var personDetails = new PersonDetails
        {
            CdpPersonId = personId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };

        var response = membership.ToResponse(includeAssignments: false, personDetails: personDetails);

        response.FirstName.Should().Be("Test");
        response.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void UserAssignmentMappingExtensions_ToResponse_MapsDetails()
    {
        var membership = new UserOrganisationMembership
        {
            Id = 14,
            OrganisationId = 11,
            UserPrincipalId = "user-1",
            OrganisationRole = OrganisationRole.Member,
            IsActive = true,
            JoinedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero)
        };
        var orgApp = new OrganisationApplication
        {
            Id = 20,
            OrganisationId = 11,
            ApplicationId = 12,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
            CreatedBy = "system",
            Organisation = new CoreOrganisation
            {
                Id = 11,
                CdpOrganisationGuid = Guid.NewGuid(),
                Name = "Org",
                Slug = "org",
                IsActive = true
            },
            Application = new Application
            {
                Id = 12,
                Name = "App",
                ClientId = "client",
                IsActive = true
            }
        };
        var assignment = new UserApplicationAssignment
        {
            Id = 15,
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = orgApp.Id,
            IsActive = true,
            CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
            UserOrganisationMembership = membership,
            OrganisationApplication = orgApp,
            Roles = new List<ApplicationRole>
            {
                new()
                {
                    Id = 1,
                    ApplicationId = 12,
                    Name = "Role",
                    IsActive = true,
                    CreatedAt = new DateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.Zero),
                    CreatedBy = "system"
                }
            }
        };

        var response = assignment.ToResponse();

        response.UserPrincipalId.Should().Be("user-1");
        response.OrganisationId.Should().Be(11);
        response.ApplicationId.Should().Be(12);
        response.Roles.Should().ContainSingle();
    }

    [Fact]
    public void PersonDetailsMappingExtensions_ToResponse_MapsFields()
    {
        var person = new PersonDetails
        {
            CdpPersonId = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com"
        };

        var response = person.ToResponse();

        response.Email.Should().Be("test@example.com");
        response.CdpPersonId.Should().Be(person.CdpPersonId);
    }
}
