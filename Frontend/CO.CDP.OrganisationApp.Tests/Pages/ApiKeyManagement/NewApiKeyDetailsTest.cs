using CO.CDP.OrganisationApp.Pages.ApiKeyManagement;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.ApiKeyManagement;

public class NewApiKeyDetailsTest
{
    private readonly Mock<ISession> _session = new();

    private NewApiKeyDetailsModel CreateModel(Guid id) => new(_session.Object) { Id = id };

    [Fact]
    public void OnGet_WhenKeyInSession_ShouldReturnPageWithKeyAndRemoveItFromSession()
    {
        _session.Setup(s => s.Get<string>(Session.NewApiKeyKey)).Returns("test-api-key");
        var pageModel = CreateModel(Guid.NewGuid());

        var result = pageModel.OnGet();

        result.Should().BeOfType<PageResult>();
        pageModel.ApiKey.Should().Be("test-api-key");
        _session.Verify(s => s.Remove(Session.NewApiKeyKey), Times.Once);
    }

    [Fact]
    public void OnGet_WhenKeyNotInSession_ShouldRedirectToManageApiKeys()
    {
        var id = Guid.NewGuid();
        _session.Setup(s => s.Get<string>(Session.NewApiKeyKey)).Returns((string?)null);
        var pageModel = CreateModel(id);

        var result = pageModel.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be($"/organisation/{id}/manage-api-keys");
        pageModel.ApiKey.Should().BeNull();
        _session.Verify(s => s.Remove(It.IsAny<string>()), Times.Never);
    }
}
