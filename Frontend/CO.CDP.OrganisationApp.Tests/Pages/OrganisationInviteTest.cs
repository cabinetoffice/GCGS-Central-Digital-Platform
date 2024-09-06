using CO.CDP.OrganisationApp.Pages;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class OrganisationInviteModelTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly OrganisationInviteModel _pageModel;

    public OrganisationInviteModelTests()
    {
        _mockSession = new Mock<ISession>();
        _pageModel = new OrganisationInviteModel(_mockSession.Object);
    }

    [Fact]
    public void OnGet_Sets_PersonInviteId_In_Session_And_Redirects_To_OneLogin()
    {
        var personInviteId = Guid.NewGuid();

        var result = _pageModel.OnGet(personInviteId);

        _mockSession.Verify(s => s.Set("PersonInviteId", personInviteId), Times.Once);

        var redirectResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/one-login/sign-in", redirectResult.Url);
    }
}