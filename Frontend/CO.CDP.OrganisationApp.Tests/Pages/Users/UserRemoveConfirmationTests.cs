using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Pages.Users;

public class UserRemoveConfirmationModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly UserRemoveConfirmationModel _pageModel;

    public UserRemoveConfirmationModelTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        _pageModel = new UserRemoveConfirmationModel(_mockOrganisationClient.Object)
        {
            Id = Guid.NewGuid(),
            PersonId = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task OnGet_Returns_Page_When_Person_Found()
    {
        var person = new Organisation.WebApiClient.Person ("john@johnson.com", "John", _pageModel.PersonId, "Johnson", []);
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ReturnsAsync(new List<Organisation.WebApiClient.Person> { person });

        var result = await _pageModel.OnGet();

        Assert.IsType<PageResult>(result);
        Assert.Equal("John Johnson", _pageModel.UserFullName);
    }

    [Fact]
    public async Task OnGet_Redirects_To_PageNotFound_When_Person_Not_Found()
    {
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ReturnsAsync(new List<Organisation.WebApiClient.Person>());

        var result = await _pageModel.OnGet();

        var redirectToPageResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/page-not-found", redirectToPageResult.Url);
    }

    [Fact]
    public async Task OnPost_Returns_Page_When_ModelState_Is_Invalid()
    {
        _pageModel.ModelState.AddModelError("ConfirmRemove", "Required");

        var result = await _pageModel.OnPost();

        Assert.IsType<PageResult>(result);
    }

    [Fact]
    public async Task OnPost_Redirects_To_UserSummary_When_ConfirmRemove_Is_True_And_Person_Exists()
    {
        _pageModel.ConfirmRemove = true;
        var person = new Organisation.WebApiClient.Person ("john@johnson.com", "John", _pageModel.PersonId, "Johnson", []);
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ReturnsAsync(new List<Organisation.WebApiClient.Person> { person });

        var result = await _pageModel.OnPost();

        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectToPageResult.PageName);
        Assert.Equal(_pageModel.Id, redirectToPageResult.RouteValues["Id"]);
        _mockOrganisationClient.Verify(client => client.RemovePersonFromOrganisationAsync(_pageModel.Id, _pageModel.PersonId), Times.Once);
    }

    [Fact]
    public async Task OnPost_Redirects_To_PageNotFound_When_ConfirmRemove_Is_True_But_Person_Not_Found()
    {
        _pageModel.ConfirmRemove = true;
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ReturnsAsync(new List<Organisation.WebApiClient.Person>());

        var result = await _pageModel.OnPost();

        var redirectToPageResult = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/page-not-found", redirectToPageResult.Url);
    }

    [Fact]
    public async Task OnPost_Redirects_To_UserSummary_When_ConfirmRemove_Is_False()
    {
        _pageModel.ConfirmRemove = false;

        var result = await _pageModel.OnPost();

        var redirectToPageResult = Assert.IsType<RedirectToPageResult>(result);
        Assert.Equal("UserSummary", redirectToPageResult.PageName);
        Assert.Equal(_pageModel.Id, redirectToPageResult.RouteValues["Id"]);
        _mockOrganisationClient.Verify(client => client.RemovePersonFromOrganisationAsync(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetPerson_Returns_Person_When_Found()
    {
        var person = new Organisation.WebApiClient.Person ("john@johnson.com", "John", _pageModel.PersonId, "Johnson", []);
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ReturnsAsync(new List<Organisation.WebApiClient.Person> { person });

        var result = await _pageModel.GetPerson(_mockOrganisationClient.Object);

        Assert.Equal(person, result);
    }

    [Fact]
    public async Task GetPerson_Returns_Null_When_Not_Found()
    {
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ReturnsAsync(new List<Organisation.WebApiClient.Person>());

        var result = await _pageModel.GetPerson(_mockOrganisationClient.Object);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPerson_Returns_Null_When_ApiException_Is_Thrown_With_404()
    {
        _mockOrganisationClient.Setup(client => client.GetOrganisationPersonsAsync(It.IsAny<Guid>()))
                               .ThrowsAsync(new ApiException("Not Found", 404, "Not Found", null, null));

        var result = await _pageModel.GetPerson(_mockOrganisationClient.Object);

        Assert.Null(result);
    }
}
