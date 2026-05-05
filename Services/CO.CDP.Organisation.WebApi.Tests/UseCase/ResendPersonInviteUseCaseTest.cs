using CO.CDP.GovUKNotify;
using CO.CDP.GovUKNotify.Models;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Persistence = CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase;

public class ResendPersonInviteUseCaseTest
{
    private readonly IConfiguration _configuration;
    private readonly Mock<IGovUKNotifyApiClient> _govUkNotifyApiClient = new();
    private readonly Mock<Persistence.IOrganisationRepository> _organisationRepository = new();
    private readonly Mock<Persistence.IPersonInviteRepository> _personInviteRepository = new();

    public ResendPersonInviteUseCaseTest()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("GOVUKNotify:PersonInviteEmailTemplateId", "test-template-id"),
                new KeyValuePair<string, string?>("OrganisationAppUrl", "http://baseurl/"),
            })
            .Build();
    }

    private ResendPersonInviteUseCase UseCase => new(
        _organisationRepository.Object,
        _personInviteRepository.Object,
        _govUkNotifyApiClient.Object,
        _configuration);

    private static Persistence.Organisation MakeOrg(int id, Guid guid, string name) =>
        new()
        {
            Id = id,
            Guid = guid,
            Name = name,
            Type = OrganisationType.Organisation,
            Tenant = new Persistence.Tenant { Guid = Guid.NewGuid(), Name = name }
        };

    private static Persistence.PersonInvite MakeInvite(int id, Guid guid, int organisationId) =>
        new()
        {
            Id = id,
            Guid = guid,
            OrganisationId = organisationId,
            Email = "user@example.com",
            FirstName = "Jane",
            LastName = "Doe",
            Scopes = []
        };

    [Fact]
    public async Task Execute_ValidInvite_ClearsExpiresOnAndUpdatesInviteSentOn()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = MakeOrg(1, orgGuid, "Test Org");
        var invite = MakeInvite(10, inviteGuid, organisation.Id);
        invite.ExpiresOn = DateTimeOffset.UtcNow.AddDays(-1);

        _organisationRepository.Setup(r => r.Find(orgGuid)).ReturnsAsync(organisation);
        _personInviteRepository.Setup(r => r.Find(inviteGuid)).ReturnsAsync(invite);

        var before = DateTimeOffset.UtcNow;
        await UseCase.Execute((orgGuid, inviteGuid));
        var after = DateTimeOffset.UtcNow;

        invite.ExpiresOn.Should().BeNull();
        invite.InviteSentOn.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task Execute_ValidInvite_SavesInviteAndSendsEmail()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = MakeOrg(1, orgGuid, "Test Org");
        var invite = MakeInvite(10, inviteGuid, organisation.Id);

        _organisationRepository.Setup(r => r.Find(orgGuid)).ReturnsAsync(organisation);
        _personInviteRepository.Setup(r => r.Find(inviteGuid)).ReturnsAsync(invite);

        await UseCase.Execute((orgGuid, inviteGuid));

        _personInviteRepository.Verify(r => r.Save(invite), Times.Once);
        _govUkNotifyApiClient.Verify(c => c.SendEmail(It.Is<EmailNotificationRequest>(req =>
            req.EmailAddress == invite.Email &&
            req.TemplateId == "test-template-id" &&
            req.Personalisation["first_name"] == "Jane" &&
            req.Personalisation["last_name"] == "Doe" &&
            req.Personalisation["org_name"] == "Test Org" &&
            req.Personalisation["invite_link"].Contains(inviteGuid.ToString())
        )), Times.Once);
    }

    [Fact]
    public async Task Execute_UnknownOrganisation_ThrowsUnknownOrganisationException()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        _organisationRepository.Setup(r => r.Find(orgGuid)).ReturnsAsync((Persistence.Organisation?)null);

        var act = async () => await UseCase.Execute((orgGuid, inviteGuid));

        await act.Should().ThrowAsync<UnknownOrganisationException>();
    }

    [Fact]
    public async Task Execute_UnknownInvite_ThrowsUnknownInvitedPersonException()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = MakeOrg(1, orgGuid, "Test Org");
        _organisationRepository.Setup(r => r.Find(orgGuid)).ReturnsAsync(organisation);
        _personInviteRepository.Setup(r => r.Find(inviteGuid)).ReturnsAsync((Persistence.PersonInvite?)null);

        var act = async () => await UseCase.Execute((orgGuid, inviteGuid));

        await act.Should().ThrowAsync<UnknownInvitedPersonException>();
    }

    [Fact]
    public async Task Execute_InviteBelongsToDifferentOrganisation_ThrowsUnknownInvitedPersonException()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = MakeOrg(1, orgGuid, "Test Org");
        var invite = MakeInvite(10, inviteGuid, 999);

        _organisationRepository.Setup(r => r.Find(orgGuid)).ReturnsAsync(organisation);
        _personInviteRepository.Setup(r => r.Find(inviteGuid)).ReturnsAsync(invite);

        var act = async () => await UseCase.Execute((orgGuid, inviteGuid));

        await act.Should().ThrowAsync<UnknownInvitedPersonException>();
    }

    [Fact]
    public async Task Execute_ValidInvite_PreservesOriginalInviteGuid()
    {
        var orgGuid = Guid.NewGuid();
        var inviteGuid = Guid.NewGuid();
        var organisation = MakeOrg(1, orgGuid, "Test Org");
        var invite = MakeInvite(10, inviteGuid, organisation.Id);

        _organisationRepository.Setup(r => r.Find(orgGuid)).ReturnsAsync(organisation);
        _personInviteRepository.Setup(r => r.Find(inviteGuid)).ReturnsAsync(invite);

        await UseCase.Execute((orgGuid, inviteGuid));

        invite.Guid.Should().Be(inviteGuid);
    }
}