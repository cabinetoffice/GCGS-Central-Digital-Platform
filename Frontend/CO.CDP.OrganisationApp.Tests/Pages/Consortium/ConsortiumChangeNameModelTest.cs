using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumChangeNameModelTest
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly ConsortiumChangeNameModel model;

    public ConsortiumChangeNameModelTest()
    {
        organisationClientMock = new();
        model = new ConsortiumChangeNameModel(organisationClientMock.Object);
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenOrganisationNameIsEmpty_ShouldRaiseOrganisationNameValidationError()
    {
        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("ConsortiumName")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("ConsortiumName")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Consortium_ConsortiumName_EnterNameError);
    }

    [Fact]
    public void WhenOrganisationNameIsNotEmpty_ShouldNotRaiseOrganisationNameValidationError()
    {
        model.ConsortiumName = "dummay";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("OrganisationName")).Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<Task<IActionResult>>();
    }

    [Fact]
    public async Task OnPost_WhenValidModel_ShouldSaveOrganisationName()
    {
        var id = Guid.NewGuid();
        model.Id = id;
        model.ConsortiumName = "updated";
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var result = await model.OnPost();
        result.Should().NotBeNull();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumOverview");
    }

    [Fact]
    public async Task OnGet_ValidSession_ReturnsConsortiumDetailsAsync()
    {
        var id = Guid.NewGuid();
        model.Id = id;
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await model.OnGet();

        organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }

    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: id ?? Guid.NewGuid(), identifier: null, name: "Test Org", type: OrganisationType.Organisation, roles: [], details: new Details(approval: null, pendingRoles: []));
    }
}