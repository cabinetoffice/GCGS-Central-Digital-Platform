using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationEmailModelTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationEmailModelTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenOrganisationEmailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenEmailIsEmpty_ShouldRaiseEmailAddressValidationError()
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

        var results = ModelValidationHelper.Validate(model, validationContext);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeTrue();

        results.Where(c => c.MemberNames.Contains("EmailAddress")).First()
            .ErrorMessage.Should().Be(StaticTextResource.Organisation_Email_Required_ErrorMessage);
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

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegistrationDetailsInSession()
    {
        var model = GivenOrganisationEmailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnPost();

        sessionMock.Verify(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey), Times.Once);
        sessionMock.Verify(s => s.Set(Session.RegistrationDetailsKey, It.IsAny<RegistrationDetails>()), Times.Once);
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var model = GivenOrganisationEmailModel();
        model.OnGet();

        model.EmailAddress.Should().Be(registrationDetails.OrganisationEmailAddress);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationAddressPage()
    {
        var model = GivenOrganisationEmailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationRegisteredAddress");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenOrganisationEmailModel();
        model.RedirectToSummary = true;

        RegistrationDetails registrationDetails = DummyRegistrationDetails();
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationScheme = "TestType",
            OrganisationEmailAddress = "test@example.com"
        };

        return registrationDetails;
    }

    private OrganisationEmailModel GivenOrganisationEmailModel()
    {
        return new OrganisationEmailModel(sessionMock.Object);
    }
}