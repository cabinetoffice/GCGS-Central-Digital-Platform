using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Users;
using CO.CDP.Person.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProblemDetails = CO.CDP.Person.WebApiClient.ProblemDetails;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class ClaimOrganisationInviteModelTests
{
    private readonly Mock<IPersonClient> personClientMock;
    private readonly Mock<ISession> sessionMock;
    private readonly ClaimOrganisationInviteModel model;
    private const string UsreUrn = "urn:test";
    private readonly Guid PersonId = Guid.NewGuid();

    public ClaimOrganisationInviteModelTests()
    {
        var person = new Person.WebApiClient.Person("test@test", "F1", PersonId, "L1", new List<string>());
        personClientMock = new Mock<IPersonClient>();
        personClientMock.Setup(pc => pc.LookupPersonAsync(UsreUrn)).ReturnsAsync(person);

        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = UsreUrn });

        model = new ClaimOrganisationInviteModel(personClientMock.Object, sessionMock.Object);
    }

    [Fact]
    public async Task OnGet_ShouldRedirectToOrganisationSelection_WhenPersonAndInviteClaimedSuccessfully()
    {
        var personInviteId = Guid.NewGuid();
        var claimPersonInvite = new ClaimPersonInvite(personInviteId);
        personClientMock.Setup(pc => pc.ClaimPersonInviteAsync(PersonId, claimPersonInvite)).Returns(Task.CompletedTask);

        var result = await model.OnGet(personInviteId);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
        personClientMock.Verify(pc => pc.LookupPersonAsync(UsreUrn), Times.Once);
        personClientMock.Verify(pc => pc.ClaimPersonInviteAsync(PersonId, claimPersonInvite), Times.Once);
    }

    [Fact]
    public async Task OnGet_ShouldStillRedirectToOrganisationSelection_WhenPersonAlreadyAddedToOrganisation()
    {
        var personInviteId = Guid.NewGuid();
        var claimPersonInvite = new ClaimPersonInvite(personInviteId);
        var problemDetails = new ProblemDetails("", "", null, "", "") { AdditionalProperties = { { "code", "PERSON_ALREADY_ADDED_TO_ORGANISATION" } } };
        personClientMock.Setup(pc => pc.ClaimPersonInviteAsync(PersonId, new ClaimPersonInvite(personInviteId)))
            .ThrowsAsync(new ApiException<ProblemDetails>("You are already a member of this organisation.", 400, "", null, problemDetails, null));

        var result = await model.OnGet(personInviteId);

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("../Organisation/OrganisationSelection");
    }

    [Fact]
    public async Task OnGet_ShouldThrowForOtherApiErrors()
    {
        var personInviteId = Guid.NewGuid();
        var problemDetails = new ProblemDetails("", "", null, "", "") { AdditionalProperties = { { "code", "SOME_OTHER_ERROR" } } };
        personClientMock.Setup(pc => pc.ClaimPersonInviteAsync(PersonId, new ClaimPersonInvite(personInviteId)))
            .ThrowsAsync(new ApiException<ProblemDetails>("Some other error", 400, "", null, problemDetails, null));

        Func<Task> act = async () => await model.OnGet(personInviteId);

        await act.Should().ThrowAsync<ApiException<ProblemDetails>>();
    }
}