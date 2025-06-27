using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationRemovePageTests
{
    private readonly ChildOrganisationRemovePage _model = new();

    [Fact]
    public void OnGet_ShouldNotModifyState()
    {
        var initialId = Guid.NewGuid();
        var initialChildId = Guid.NewGuid();

        _model.Id = initialId;
        _model.ChildId = initialChildId;
        _model.RemoveConfirmation = false;

        _model.OnGet();

        _model.Id.Should().Be(initialId);
        _model.ChildId.Should().Be(initialChildId);
        _model.RemoveConfirmation.Should().BeFalse();
    }

    [Fact]
    public void OnPost_WithInvalidModelState_ShouldReturnPage()
    {
        _model.ModelState.AddModelError("RemoveConfirmation", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WithRemoveConfirmationTrue_ShouldCallDelete()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.RemoveConfirmation = true;

        var result = _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be($"/organisation/{id}");
        redirectResult.RouteValues.Should().NotBeNull();
        redirectResult.RouteValues!.Should().ContainKey("childRemoved");
        redirectResult.RouteValues!["childRemoved"].Should().Be(true);
    }

    [Fact]
    public void OnPost_WithRemoveConfirmationFalse_ShouldRedirectToOrganisationPage()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.RemoveConfirmation = false;

        var result = _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be($"/organisation/{id}");
        redirectResult.RouteValues.Should().BeNull();
    }
}
