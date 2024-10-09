using Moq;
using Microsoft.AspNetCore.Mvc;
using CO.CDP.OrganisationApp.Pages.Users;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Tests.Pages.Users;

public class AddUserModelTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly AddUserModel _addUserModel;

    public AddUserModelTests()
    {
        _mockSession = new Mock<ISession>();
        _addUserModel = new AddUserModel(_mockSession.Object);
    }

    [Fact]
    public void OnGet_ShouldInitializeModel_WhenPersonInviteStateExists()
    {
        PersonInviteState? personInviteState = new PersonInviteState
        {
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com",
            Scopes = new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor }
        };

        _mockSession
            .Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey))
            .Returns(personInviteState);

        var result = _addUserModel.OnGet();

        Assert.Equal("John", _addUserModel.FirstName);
        Assert.Equal("Johnson", _addUserModel.LastName);
        Assert.Equal("john@johnson.com", _addUserModel.Email);
        Assert.True(_addUserModel.IsAdmin);
        Assert.Equal(OrganisationPersonScopes.Editor, _addUserModel.Role);
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnGet_ShouldNotInitializeModel_WhenPersonInviteStateDoesNotExist()
    {
        _mockSession.Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey)).Returns((PersonInviteState)null!);

        var result = _addUserModel.OnGet();

        Assert.Null(_addUserModel.FirstName);
        Assert.Null(_addUserModel.LastName);
        Assert.Null(_addUserModel.Email);
        Assert.Null(_addUserModel.IsAdmin);
        Assert.Null(_addUserModel.Role);
        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnPost_ShouldReturnPageResult_WhenModelStateIsInvalid()
    {
        _addUserModel.ModelState.AddModelError("FirstName", "Required");

        var result = _addUserModel.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionAndRedirect_WhenModelStateIsValid()
    {
        _addUserModel.FirstName = "John";
        _addUserModel.LastName = "Johnson";
        _addUserModel.Email = "john@johnson.com";
        _addUserModel.Role = OrganisationPersonScopes.Editor;

        var initialState = new PersonInviteState();
        _mockSession.Setup(s => s.Get<PersonInviteState>(PersonInviteState.TempDataKey)).Returns(initialState);

        var result = _addUserModel.OnPost();

        _mockSession.Verify(s => s.Set(PersonInviteState.TempDataKey, It.Is<PersonInviteState>(state =>
            state.Scopes != null &&
            state.FirstName == "John" &&
            state.LastName == "Johnson" &&
            state.Email == "john@johnson.com" &&
            state.Scopes.Contains(OrganisationPersonScopes.Editor)
        )), Times.Once);

        var redirectResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserCheckAnswers", redirectResult.PageName);
        Assert.Equal(_addUserModel.Id, redirectResult.RouteValues?["Id"]);
    }

    [Fact]
    public void UpdateFields_ShouldUpdateAllFields_WhenFieldsAreNotEmpty()
    {
        _addUserModel.FirstName = "John";
        _addUserModel.LastName = "Johnson";
        _addUserModel.Email = "john@johnson.com";

        var initialState = new PersonInviteState();

        var updatedState = _addUserModel.UpdateFields(initialState);

        Assert.Equal("John", updatedState.FirstName);
        Assert.Equal("Johnson", updatedState.LastName);
        Assert.Equal("john@johnson.com", updatedState.Email);
    }

    [Theory]
    [InlineData(OrganisationPersonScopes.Viewer)]
    [InlineData(OrganisationPersonScopes.Editor)]
    [InlineData(OrganisationPersonScopes.Admin)]
    public void UpdateScopes_ShouldUpdateScopes_WhenRoleIsSelected(string organisationPersonScope)
    {
        _addUserModel.Role = organisationPersonScope;

        var initialState = new PersonInviteState
        {
            Scopes = new List<string> { OrganisationPersonScopes.Viewer }
        };

        var updatedState = _addUserModel.UpdateScopes(initialState);

        Assert.Contains(organisationPersonScope, updatedState.Scopes ?? []);
    }

    [Fact]
    public void InitModel_ShouldSetModelPropertiesFromState_WhenStateHasData()
    {
        var state = new PersonInviteState
        {
            FirstName = "John",
            LastName = "Johnson",
            Email = "john@johnson.com",
            Scopes = new List<string> { OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor }
        };

        _addUserModel.InitModel(state);

        Assert.Equal("John", _addUserModel.FirstName);
        Assert.Equal("Johnson", _addUserModel.LastName);
        Assert.Equal("john@johnson.com", _addUserModel.Email);
        Assert.True(_addUserModel.IsAdmin);
        Assert.Equal(OrganisationPersonScopes.Editor, _addUserModel.Role);
    }
}