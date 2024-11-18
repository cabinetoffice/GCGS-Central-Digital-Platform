using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationEmailModelTest
{
    private readonly Mock<IOrganisationClient> organisationClientMock;
    private readonly OrganisationEmailModel _model;
    public OrganisationEmailModelTest()
    {
        organisationClientMock = new Mock<IOrganisationClient>();
        _model = new OrganisationEmailModel(organisationClientMock.Object);
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationEmailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenOrganisationNameIsEmpty_ShouldRaiseOrganisationEmailValidationError()
    {
        var model = GivenOrganisationEmailModel();

        var stringLocalizerMock = new Mock<IStringLocalizer>();
        stringLocalizerMock
            .Setup(localizer => localizer[nameof(StaticTextResource.Organisation_Email_Required_ErrorMessage)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Organisation_Email_Required_ErrorMessage), StaticTextResource.Organisation_Email_Required_ErrorMessage));

        var stringLocalizerFactoryMock = new Mock<IStringLocalizerFactory>();
        stringLocalizerFactoryMock
            .Setup(factory => factory.Create(It.IsAny<Type>()))
            .Returns(stringLocalizerMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IServiceProvider)))
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IStringLocalizerFactory)))
            .Returns(stringLocalizerFactoryMock.Object);

        var validationContext = new ValidationContext(model, serviceProviderMock.Object, null);

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Organisation_Email_Required_ErrorMessage);
    }

    [Fact]
    public void WhenOrganisationNameIsNotEmpty_ShouldNotRaiseOrganisationNameValidationError()
    {
        var model = GivenOrganisationEmailModel();
        model.EmailAddress = "dummay@gmail.com";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeFalse();
    }

    [Fact]
    public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationEmailModel();
        model.EmailAddress = "dummy";

        var stringLocalizerMock = new Mock<IStringLocalizer>();
        stringLocalizerMock
            .Setup(localizer => localizer[nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage)])
            .Returns(new LocalizedString(nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), StaticTextResource.Global_Email_Invalid_ErrorMessage));

        var stringLocalizerFactoryMock = new Mock<IStringLocalizerFactory>();
        stringLocalizerFactoryMock
            .Setup(factory => factory.Create(It.IsAny<Type>()))
            .Returns(stringLocalizerMock.Object);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IServiceProvider)))
            .Returns(serviceProviderMock.Object);

        serviceProviderMock
            .Setup(provider => provider.GetService(typeof(IStringLocalizerFactory)))
            .Returns(stringLocalizerFactoryMock.Object);

        var validationContext = new ValidationContext(model, serviceProviderMock.Object, null);

        var results = ModelValidationHelper.Validate(model, validationContext);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Global_Email_Invalid_ErrorMessage);
    }

    [Fact]
    public void WhenEmailAddressIsValid_ShouldNotRaiseEmailAddressValidationError()
    {
        var model = GivenOrganisationEmailModel();
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

        var model = GivenOrganisationEmailModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<Task<IActionResult>>();
    }

    [Fact]
    public async Task OnPost_WhenValidModel_ShouldSaveOrganisationEmail()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        _model.EmailAddress = "updated@test.com";
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        var result = await _model.OnPost();
        result.Should().NotBeNull();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationOverview");
    }

    [Fact]
    public async Task OnGet_ValidSession_ReturnsOrganisationDetailsAsync()
    {
        var id = Guid.NewGuid();
        _model.Id = id;
        organisationClientMock.Setup(o => o.GetOrganisationAsync(id))
            .ReturnsAsync(GivenOrganisationClientModel(id));

        await _model.OnGet();

        organisationClientMock.Verify(c => c.GetOrganisationAsync(id), Times.Once);
    }


    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel(Guid? id)
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: new ContactPoint("Main Contact", "contact@test.com", "123456789", null), id: id ?? Guid.NewGuid(), identifier: null, name: null, roles: [], details: new Details(approval: null, pendingRoles: []));
    }

    private OrganisationEmailModel GivenOrganisationEmailModel()
    {
        return new OrganisationEmailModel(organisationClientMock.Object);
    }
}