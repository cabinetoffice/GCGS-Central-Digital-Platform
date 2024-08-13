using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityDeclarationTest
{
    [Fact]
    public void OnPost_ShouldRedirectToConnectedQuestionPageWithId()
    {
        var model = new ConnectedEntityDeclarationModel { Id = Guid.NewGuid() };

        var result = model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("ConnectedEntitySupplierHasControl");

    }
}