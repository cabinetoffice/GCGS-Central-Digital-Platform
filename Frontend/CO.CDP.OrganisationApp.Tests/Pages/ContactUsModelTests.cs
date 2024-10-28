using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages;

public class ContactUsModelTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient = new();
    private readonly ContactUsModel _pageModel;

    public ContactUsModelTests()
    {
        _pageModel = new ContactUsModel(_mockOrganisationClient.Object)
        {
            EmailAddress = "test@example.com",
            Message = "message",
            Name = "User",
            OrganisationName = "Organisation"
        };
    }

    [Fact]
    public async Task OnPost_InvalidModelState_ReturnsPageResult()
    {
        _pageModel.ModelState.AddModelError("Error", "Invalid");

        var result = await _pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_ValidModelState_ShouldCallContactUsAndRedirect()
    {
        _mockOrganisationClient.Setup(client => client.ContactUsAsync(It.IsAny<ContactUs>())).ReturnsAsync(true);

        var result = await _pageModel.OnPost();

        _mockOrganisationClient.Verify(client => client.ContactUsAsync(It.IsAny<ContactUs>()), Times.Once);
        result.Should().BeOfType<RedirectResult>()
              .Which.Url.Should().Be("/contact-us?message-sent=true");
    }
}