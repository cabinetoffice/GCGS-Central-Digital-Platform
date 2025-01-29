using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumChangeEmailModelTest
{
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private readonly ConsortiumChangeEmailModel _model;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;

    public ConsortiumChangeEmailModelTest()
    {
        _organisationClientMock = new Mock<IOrganisationClient>();
        _model = new ConsortiumChangeEmailModel(_organisationClientMock.Object);
        _stringLocalizerMock = new Mock<IStringLocalizer>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenConsortiumChangeEmailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenConsortiumNameIsEmpty_ShouldRaiseConsortiumEmailValidationError()
    {
        var model = GivenConsortiumChangeEmailModel();

        _stringLocalizerMock
            .Setup(localizer => localizer[nameof(StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage), StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage));

        var validationContext = ValidationContextFactory.GivenValidationContextWithStringLocalizerFactory(model, _stringLocalizerMock.Object);

        var results = ModelValidationHelper.Validate(model, validationContext);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Consortium_ConsortiumEmail_Required_ErrorMessage);
    }

    [Fact]
    public void WhenConsortiumEmailIsNotEmpty_ShouldNotRaiseConsortiumEmailValidationError()
    {
        var model = GivenConsortiumChangeEmailModel();
        model.EmailAddress = "dummay@gmail.com";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeFalse();
    }

    [Fact]
    public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenConsortiumChangeEmailModel();
        model.EmailAddress = "dummy";

        _stringLocalizerMock
            .Setup(localizer => localizer[nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), StaticTextResource.Global_Email_Invalid_ErrorMessage));

        var validationContext = ValidationContextFactory.GivenValidationContextWithStringLocalizerFactory(model, _stringLocalizerMock.Object);

        var results = ModelValidationHelper.Validate(model, validationContext);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Global_Email_Invalid_ErrorMessage);
    }

    [Fact]
    public void WhenEmailAddressIsValid_ShouldNotRaiseEmailAddressValidationError()
    {
        var model = GivenConsortiumChangeEmailModel();
        model.EmailAddress = "dummay@test.com";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenConsortiumChangeEmailModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<Task<IActionResult>>();
    }

    [Fact]
    public async Task OnPost_WhenValidModel_ShouldSaveConsortiumEmail()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.EmailAddress = "updated@test.com";
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var result = await _model.OnPost();
        result.Should().NotBeNull();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumOverview");
    }

    [Fact]
    public async Task OnGet_ValidSession_ReturnsConsortiumDetailsAsync()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await _model.OnGet();

        _organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }


    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: new ContactPoint("Main Contact", "contact@test.com", "123456789", null), id: id ?? Guid.NewGuid(), identifier: null, name: null, type: OrganisationType.Organisation, roles: [], details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
    }

    private ConsortiumChangeEmailModel GivenConsortiumChangeEmailModel()
    {
        return new ConsortiumChangeEmailModel(_organisationClientMock.Object);
    }
}