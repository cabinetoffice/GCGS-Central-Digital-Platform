using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Users;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class ResendInviteModelTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly ResendInviteModel _model;

    public ResendInviteModelTests()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new ResendInviteModel(_sessionMock.Object, _organisationClientMock.Object);
    }

    [Fact]
    public async Task OnGet_WithValidPersonInviteId_ShouldLoadPersonInviteIntoSessionAndRedirectToUserCheckAnswers()
    {
        var personInvite = new PersonInviteModel(
            email: "john.doe@example.com",
            expiresOn: null,
            firstName: "John",
            id: Guid.NewGuid(),
            lastName: "Doe",
            scopes: new[] { "Scope1", "Scope2" }
        );
        _model.OrganisationId = Guid.NewGuid();
        _model.PersonInviteId = personInvite.Id;

        _organisationClientMock
            .Setup(client => client.GetOrganisationPersonInvitesAsync(_model.OrganisationId))
            .ReturnsAsync(new[] { personInvite });

        var result = await _model.OnGet(_model.PersonInviteId);

        result.Should().BeOfType<RedirectToPageResult>()
              .Which.PageName.Should().Be("UserCheckAnswers");

        var redirectResult = result as RedirectToPageResult;
        redirectResult?.RouteValues?["id"].Should().Be(_model.OrganisationId);

        _sessionMock.Verify(session => session.Set(PersonInviteState.TempDataKey, It.IsAny<PersonInviteState>()), Times.Once);

        _model.PersonInviteStateData.Should().NotBeNull();
        _model.PersonInviteStateData!.FirstName.Should().Be(personInvite.FirstName);
        _model.PersonInviteStateData.LastName.Should().Be(personInvite.LastName);
        _model.PersonInviteStateData.Email.Should().Be(personInvite.Email);
        _model.PersonInviteStateData.Scopes.Should().BeEquivalentTo(personInvite.Scopes);
    }

    [Fact]
    public async Task OnGet_NoMatchingPersonInviteId_ShouldNotSetSessionData()
    {
        _model.OrganisationId = Guid.NewGuid();
        _model.PersonInviteId = Guid.NewGuid();

        _organisationClientMock
            .Setup(client => client.GetOrganisationPersonInvitesAsync(_model.OrganisationId))
            .ReturnsAsync(new List<PersonInviteModel>());

        var result = await _model.OnGet(_model.PersonInviteId);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("UserCheckAnswers");

        _sessionMock.Verify(session => session.Set(It.IsAny<string>(), It.IsAny<PersonInviteState>()), Times.Never);
        _model.PersonInviteStateData.Should().BeNull();
    }

    [Fact]
    public async Task GetPersonInvite_WithMatchingInviteId_ShouldReturnPersonInvite()
    {
        var personInvite = new PersonInviteModel(
            email: "john.doe@example.com",
            expiresOn: null,
            firstName: "John",
            id: Guid.NewGuid(),
            lastName: "Doe",
            scopes: new[] { "Scope1" }
        );
        _model.OrganisationId = Guid.NewGuid();
        _model.PersonInviteId = personInvite.Id;

        _organisationClientMock
            .Setup(client => client.GetOrganisationPersonInvitesAsync(_model.OrganisationId))
            .ReturnsAsync(new[] { personInvite });

        var result = await _model.GetPersonInvite();

        result.Should().NotBeNull();
        result!.Id.Should().Be(personInvite.Id);
        result.FirstName.Should().Be(personInvite.FirstName);
        result.LastName.Should().Be(personInvite.LastName);
        result.Email.Should().Be(personInvite.Email);
        result.Scopes.Should().BeEquivalentTo(personInvite.Scopes);
    }

    [Fact]
    public async Task GetPersonInvite_WhenApiThrows404_ShouldReturnNull()
    {
        _model.OrganisationId = Guid.NewGuid();
        _model.PersonInviteId = Guid.NewGuid();

        _organisationClientMock
            .Setup(client => client.GetOrganisationPersonInvitesAsync(_model.OrganisationId))
            .ThrowsAsync(new ApiException(
                message: "Not Found",
                statusCode: 404,
                response: string.Empty,
                headers: new Dictionary<string, IEnumerable<string>>(),
                innerException: null
            ));

        var result = await _model.GetPersonInvite();

        result.Should().BeNull();
    }
}
