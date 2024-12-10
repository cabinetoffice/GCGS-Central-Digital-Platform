using Amazon.S3;
using Amazon.S3.Model;
using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.CharityCommission;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Consortium;
using CO.CDP.OrganisationApp.Pages.Registration;
using CO.CDP.OrganisationApp.Pages.Supplier;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumEmailModelTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IStringLocalizer> _stringLocalizerMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private static readonly Guid _consortiumId = Guid.NewGuid();
    public ConsortiumEmailModelTest()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
        _stringLocalizerMock = new Mock<IStringLocalizer>();
    }

    [Fact]
    public void WhenEmptyModelIsPosted_ShouldRaiseValidationErrors()
    {
        var model = GivenConsortiumEmailModel();

        var results = ModelValidationHelper.Validate(model);

        results.Count.Should().Be(1);
    }

    [Fact]
    public void WhenEmailIsEmpty_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenConsortiumEmailModel();

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
    public void WhenEmailAddressIsInvalid_ShouldRaiseEmailAddressValidationError()
    {
        var model = GivenConsortiumEmailModel();
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
        var model = GivenConsortiumEmailModel();
        model.EmailAddress = "dummay@test.com";

        var results = ModelValidationHelper.Validate(model);

        results.Any(c => c.MemberNames.Contains("EmailAddress")).Should().BeFalse();
    }

    

    [Fact]
    public void OnGet_ValidSession_ReturnsConsortiumDetails()
    {
        ConsortiumDetails consortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey)).Returns(consortiumDetails);

        var model = GivenConsortiumEmailModel();
        model.OnGet();

        model.EmailAddress.Should().Be(consortiumDetails.ConstortiumEmail);
    }

    [Fact]
    public async Task OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenConsortiumEmailModel();
        model.PageContext = pageContext;

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public async Task OnPost_WhenValidModel_ShouldSetConsortiumDetailsInSession()
    {
        var model = GivenConsortiumEmailModel();

        ConsortiumDetails consortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey)).Returns(consortiumDetails);

        await model.OnPost();

        _sessionMock.Verify(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey), Times.Once);
        _sessionMock.Verify(s => s.Set(Session.ConsortiumKey, It.IsAny<ConsortiumDetails>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_ValidSession_ShouldRegisterConsortium()
    {
        var dummyConsortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Set(Session.ConsortiumKey, dummyConsortiumDetails));

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(dummyConsortiumDetails);

        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        _organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenConsortiumEmailDetailModel();

        model.EmailAddress = dummyConsortiumDetails.ConstortiumEmail;

        await model.OnPost();

        _organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_OnSuccess_RedirectsToOrganisationOverView()
    {
        var dummyConsortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Set(Session.ConsortiumKey, dummyConsortiumDetails));

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(dummyConsortiumDetails);

        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        _organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenConsortiumEmailDetailModel();

        model.EmailAddress = dummyConsortiumDetails.ConstortiumEmail;

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumOverview");
    }

    private ConsortiumDetails DummyConsortiumDetails(string consortiumName = "Consortium 1",        
        string consortiumEmailAddress = "test@example.com")
    {
        var consortiumDetails = new ConsortiumDetails
        {
            ConstortiumName = consortiumName,
            ConstortiumEmail = consortiumEmailAddress,
            PostalAddress = new Models.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", CountryName = "United kindom", Country = "UK" }
        };

        return consortiumDetails;
    }


    private ConsortiumEmailModel GivenConsortiumEmailModel()
    {
        return new ConsortiumEmailModel(_sessionMock.Object, _organisationClientMock.Object);
    }
    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: _consortiumId, identifier: null, name: "Test Org", type: CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium, roles: [], details: new Details(approval: null, pendingRoles: []));
    }
    private ConsortiumEmailModel GivenConsortiumEmailDetailModel()
    {
        var registrationDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(registrationDetails);

        return new ConsortiumEmailModel(_sessionMock.Object, _organisationClientMock.Object);
    }
}