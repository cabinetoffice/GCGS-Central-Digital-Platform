using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationSelectionTest
{
    private readonly Mock<ISession> sessionMock;
    private readonly Mock<IOrganisationClient> organisationClientMock;

    public OrganisationSelectionTest()
    {
        sessionMock = new Mock<ISession>();
        organisationClientMock = new Mock<IOrganisationClient>();
    }

    [Fact]
    public async Task OnGet_WhenRegistrationDetailsNotInSession_ShouldThrowException()
    {
        var model = GivenOrganisationSelectionModelModel();

        Func<Task> act = model.OnGet;

        await act.Should().ThrowAsync<Exception>().WithMessage("Shoudn't be here");
    }

    [Fact]
    public async Task OnGet_WhenSessionIsValid_ShouldPopulatePageModel()
    {
        var model = GivenOrganisationSelectionModelModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { UserUrn = "urn:test" });

        organisationClientMock.Setup(o => o.ListOrganisationsAsync(It.IsAny<string>()))
            .ReturnsAsync([new Organisation.WebApiClient.Organisation(null, null, null, Guid.NewGuid(), null, "Test Org", [1])]);

        var actionResult = await model.OnGet();

        model.Organisations.Should().HaveCount(1);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRegisterPerson()
    {
        var model = GivenOrganisationSelectionModelModel();

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationType");
    }

    private OrganisationSelectionModel GivenOrganisationSelectionModelModel()
    {
        return new OrganisationSelectionModel(organisationClientMock.Object, sessionMock.Object);
    }
}