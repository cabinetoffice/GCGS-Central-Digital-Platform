using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationSelectionTest
{
    [Fact]
    public void OnPost_WhenValidModel_ShouldRegisterPerson()
    {
        var model = new OrganisationSelectionModel();

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationType");
    }
}