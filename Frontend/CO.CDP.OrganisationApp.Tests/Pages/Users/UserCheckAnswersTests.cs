using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Users;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class UserCheckAnswersModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly Mock<ITempDataService> _mockTempDataService;
    private readonly Mock<ISession> _mockSession;
    private readonly UserCheckAnswersModel _pageModel;

    public UserCheckAnswersModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _mockSession = new Mock<ISession>();
        _mockTempDataService = new Mock<ITempDataService>();
        _pageModel = new UserCheckAnswersModel(_mockOrganisationClient.Object, _mockTempDataService.Object, _mockSession.Object);
    }

    [Fact]
    public void OnGet_PersonInviteStateDataIsNull_ShouldRedirectToAddUserPage()
    {
        _mockSession.Setup(s => s.Get<PersonInviteState>(It.IsAny<string>())).Returns((PersonInviteState)null!);

        var result = _pageModel.OnGet();

        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("AddUser", redirectToPageResult.PageName);
        Assert.Equal(_pageModel.Id, redirectToPageResult.RouteValues?["Id"]);
    }

    [Fact]
    public void OnGet_PersonInviteStateDataIsInvalid_ShouldRedirectToAddUserPage()
    {
        var invalidInviteState = new PersonInviteState { Email = "", FirstName = "", LastName = "", Scopes = null };
        _mockSession.Setup(s => s.Get<PersonInviteState>(It.IsAny<string>())).Returns(invalidInviteState);

        var result = _pageModel.OnGet();

        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("AddUser", redirectToPageResult.PageName);
        Assert.Equal(_pageModel.Id, redirectToPageResult.RouteValues?["Id"]);
    }

    [Fact]
    public void OnGet_PersonInviteStateDataIsValid_ShouldReturnPage()
    {
        var validInviteState = new PersonInviteState { Email = "john@johnson.com", FirstName = "John", LastName = "Johnson", Scopes = ["scope1"] };
        _mockSession.Setup(s => s.Get<PersonInviteState>(It.IsAny<string>())).Returns(validInviteState);

        var result = _pageModel.OnGet();

        Assert.IsType<PageResult>(result);
        Assert.NotNull(_pageModel.PersonInviteStateData);
        Assert.Equal(validInviteState, _pageModel.PersonInviteStateData);
    }

    [Fact]
    public async Task OnPost_ModelStateIsInvalid_ShouldReturnPage()
    {
        _pageModel.ModelState.AddModelError("Key", "ModelError");

        var result = await _pageModel.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPost_PersonInviteStateDataIsNull_ShouldRedirectToAddUserPage()
    {
        _mockSession.Setup(s => s.Get<PersonInviteState>(It.IsAny<string>())).Returns((PersonInviteState)null!);

        var result = await _pageModel.OnPost();

        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("AddUser", redirectToPageResult.PageName);
        Assert.Equal(_pageModel.Id, redirectToPageResult.RouteValues?["Id"]);
    }

    [Fact]
    public async Task OnPost_PersonInviteStateDataIsValid_ShouldCreatePersonInviteAndRedirectToUserSummary()
    {
        var validInviteState = new PersonInviteState { Email = "john@johnson.com", FirstName = "John", LastName = "Johnson", Scopes = ["scope1"] };
        _mockSession.Setup(s => s.Get<PersonInviteState>(It.IsAny<string>())).Returns(validInviteState);

        var result = await _pageModel.OnPost();

        _mockOrganisationClient.Verify(c => c.CreatePersonInviteAsync(_pageModel.Id, It.IsAny<InvitePersonToOrganisation>()), Times.Once);
        _mockSession.Verify(s => s.Remove(PersonInviteState.TempDataKey), Times.Once);

        _mockTempDataService.Verify(s => s.Put(FlashMessageTypes.Success, It.Is<string>(t => t.Equals("You've sent an email invite to John Johnson"))), Times.Once);

        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectToPageResult.PageName);
        Assert.Equal(_pageModel.Id, redirectToPageResult.RouteValues?["Id"]);
    }

    [Fact]
    public void Validate_PersonInviteStateDataIsValid_ShouldReturnTrue()
    {
        var validInviteState = new PersonInviteState { Email = "john@johnson.com", FirstName = "John", LastName = "Johnson", Scopes = ["scope1"] };

        var result = UserCheckAnswersModel.Validate(validInviteState);

        Assert.True(result);
    }

    [Fact]
    public void Validate_PersonInviteStateDataIsInvalid_ShouldReturnFalse()
    {
        var invalidInviteState = new PersonInviteState { Email = "", FirstName = "", LastName = "", Scopes = null };

        var result = UserCheckAnswersModel.Validate(invalidInviteState);

        Assert.False(result);
    }
}