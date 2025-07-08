using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Buyer.Hierarchy;
using CO.CDP.OrganisationApp.Tests.TestData;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using OrganisationApiException = CO.CDP.Organisation.WebApiClient.ApiException;

namespace CO.CDP.OrganisationApp.Tests.Pages.Buyer.Hierarchy;

public class ChildOrganisationRemovePageTests
{
    private readonly Mock<IOrganisationClient> _mockOrganisationClient;
    private readonly ChildOrganisationRemovePage _modelWithMocks;
    private readonly ChildOrganisationRemovePage _modelWithoutDependencies;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _childOrganisationId = Guid.NewGuid();

    public ChildOrganisationRemovePageTests()
    {
        _mockOrganisationClient = new Mock<IOrganisationClient>();
        var mockLogger = new Mock<ILogger<ChildOrganisationRemovePage>>();

        _modelWithMocks = new ChildOrganisationRemovePage(_mockOrganisationClient.Object, mockLogger.Object)
        {
            Id = _organisationId,
            ChildId = _childOrganisationId
        };

        _modelWithoutDependencies = new ChildOrganisationRemovePage(_mockOrganisationClient.Object, mockLogger.Object)
        {
            Id = Guid.NewGuid(),
            ChildId = Guid.NewGuid()
        };
    }

    [Fact]
    public void OnGet_ShouldNotModifyState()
    {
        var initialId = Guid.NewGuid();
        var initialChildId = Guid.NewGuid();

        _modelWithoutDependencies.Id = initialId;
        _modelWithoutDependencies.ChildId = initialChildId;
        _modelWithoutDependencies.RemoveConfirmation = false;

        _ = _modelWithoutDependencies.OnGet();

        _modelWithoutDependencies.Id.Should().Be(initialId);
        _modelWithoutDependencies.ChildId.Should().Be(initialChildId);
        _modelWithoutDependencies.RemoveConfirmation.Should().BeFalse();
    }

    [Fact]
    public async Task OnPost_WithInvalidModelState_ShouldReturnPage()
    {
        _modelWithoutDependencies.ModelState.AddModelError("RemoveConfirmation", "Required");

        var result = await _modelWithoutDependencies.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WithRemoveConfirmationTrue_ShouldCallDelete()
    {
        var id = Guid.NewGuid();
        _modelWithoutDependencies.Id = id;
        _modelWithoutDependencies.RemoveConfirmation = true;

        var result = await _modelWithoutDependencies.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Organisation/OrganisationOverview");
        redirectResult.RouteValues.Should().NotBeNull();
        redirectResult.RouteValues!.Should().ContainKey("id");
        redirectResult.RouteValues!["id"].Should().Be(id);
        redirectResult.RouteValues!.Should().ContainKey("childRemoved");
        redirectResult.RouteValues!["childRemoved"].Should().Be(true);
    }

    [Fact]
    public async Task OnPost_WithRemoveConfirmationFalse_ShouldRedirectToOrganisationPage()
    {
        var id = Guid.NewGuid();
        _modelWithoutDependencies.Id = id;
        _modelWithoutDependencies.RemoveConfirmation = false;

        var result = await _modelWithoutDependencies.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Organisation/OrganisationOverview");
        redirectResult.RouteValues.Should().NotBeNull();
        redirectResult.RouteValues!.Should().ContainKey("id");
        redirectResult.RouteValues!["id"].Should().Be(id);
    }

    [Fact]
    public async Task OnGet_WhenChildOrganisationsExistAndChildIsFound_ShouldReturnPage()
    {
        var childPpon = "ABCD-1234-EFGH";
        var childIdentifier = new Identifier(
            scheme: "GB-PPON",
            id: childPpon,
            legalName: "Child Organisation",
            uri: null);

        var childOrganisation = OrganisationFactory.CreateOrganisation(
            id: _childOrganisationId,
            name: "Child Organisation",
            identifier: childIdentifier);

        var childOrganisations = new List<OrganisationSummary>
        {
            new(id: _childOrganisationId, name: "Child Organisation",
                roles: new List<PartyRole> { PartyRole.Buyer }, ppon: childPpon)
        };

        _modelWithMocks.Ppon = childPpon;

        _mockOrganisationClient
            .Setup(c => c.LookupOrganisationAsync(null, childPpon))
            .ReturnsAsync(childOrganisation);

        _mockOrganisationClient
            .Setup(c => c.GetChildOrganisationsAsync(_organisationId))
            .ReturnsAsync(childOrganisations);

        var result = await _modelWithMocks.OnGet();

        result.Should().BeOfType<PageResult>();
        _modelWithMocks.ChildOrganisation.Should().NotBeNull();
        _modelWithMocks.ChildOrganisation.Should().Be(childOrganisation);
    }

    [Fact]
    public async Task OnGet_WhenChildIsNotFound_ShouldRedirectToNotFound()
    {
        var childPpon = "EFGH-5678-IJKL";
        var childIdentifier = new Identifier(
            scheme: "GB-PPON",
            id: childPpon,
            legalName: "Child Organisation",
            uri: null);

        var childOrganisation = OrganisationFactory.CreateOrganisation(
            id: _childOrganisationId,
            name: "Child Organisation",
            identifier: childIdentifier);

        var differentChildId = Guid.NewGuid();
        var childOrganisations = new List<OrganisationSummary>
        {
            new(id: differentChildId, name: "Different Child",
                roles: new List<PartyRole> { PartyRole.Buyer }, ppon: "WXYZ-9876-MNOP")
        };

        _mockOrganisationClient
            .Setup(c => c.LookupOrganisationAsync(null, It.IsAny<string>()))
            .ReturnsAsync(childOrganisation);

        _mockOrganisationClient
            .Setup(c => c.GetChildOrganisationsAsync(_organisationId))
            .ReturnsAsync(childOrganisations);

        var result = await _modelWithMocks.OnGet();

        var redirectResult = result.Should().BeOfType<RedirectResult>().Subject;
        redirectResult.Url.Should().Be("/page-not-found");
    }

    [Fact]
    public async Task OnGet_WhenApiThrowsException_ShouldRedirectToErrorPage()
    {
        _mockOrganisationClient
            .Setup(c => c.LookupOrganisationAsync(null, It.IsAny<string>()))
            .ThrowsAsync(new OrganisationApiException("Error", 500, "Error", null, null));

        var result = await _modelWithMocks.OnGet();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Error");
    }

    [Fact]
    public async Task Delete_WhenApiCallSucceeds_ShouldRedirectWithChildRemovedFlag()
    {
        _mockOrganisationClient
            .Setup(c => c.SupersedeChildOrganisationAsync(_organisationId, _childOrganisationId))
            .Returns(Task.CompletedTask);

        _modelWithMocks.RemoveConfirmation = true;

        var result = await _modelWithMocks.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Organisation/OrganisationOverview");
        redirectResult.RouteValues.Should().NotBeNull();
        redirectResult.RouteValues!.Should().ContainKey("id");
        redirectResult.RouteValues!["id"].Should().Be(_organisationId);
        redirectResult.RouteValues!.Should().ContainKey("childRemoved");
        redirectResult.RouteValues!["childRemoved"].Should().Be(true);

        _mockOrganisationClient.Verify(c => c.SupersedeChildOrganisationAsync(_organisationId, _childOrganisationId),
            Times.Once);
    }

    [Fact]
    public async Task Delete_WhenApiCallFails_ShouldRedirectToErrorPage()
    {
        _mockOrganisationClient
            .Setup(c => c.SupersedeChildOrganisationAsync(_organisationId, _childOrganisationId))
            .ThrowsAsync(new OrganisationApiException("Error", 500, "Error", null, null));

        _modelWithMocks.RemoveConfirmation = true;

        var result = await _modelWithMocks.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("/Error");

        _mockOrganisationClient.Verify(c => c.SupersedeChildOrganisationAsync(_organisationId, _childOrganisationId),
            Times.Once);
    }
}