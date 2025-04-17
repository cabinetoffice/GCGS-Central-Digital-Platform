using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
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

public class ConsortiumCheckAnswerModelTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly Mock<IOrganisationClient> _organisationClientMock;
    private static readonly Guid _consortiumId = Guid.NewGuid();
    public ConsortiumCheckAnswerModelTest()
    {
        _sessionMock = new Mock<ISession>();
        _organisationClientMock = new Mock<IOrganisationClient>();
        _sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsConsortiumDetails()
    {
        ConsortiumDetails consortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey)).Returns(consortiumDetails);

        var model = GivenConsortiumCheckAnswerModel();
        model.OnGet();

        model.ConsortiumDetails.ConsortiumEmail.Should().Be(consortiumDetails.ConsortiumEmail);
    }

    [Fact]
    public async Task OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenConsortiumCheckAnswerModel();
        model.PageContext = pageContext;

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
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

        var model = GivenConsortiumCheckAnswerDetailModel();

        await model.OnPost();

        _organisationClientMock.Verify(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_OnSuccess_RedirectsToConsortiumOverView()
    {
        var dummyConsortiumDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Set(Session.ConsortiumKey, dummyConsortiumDetails));

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(dummyConsortiumDetails);

        _sessionMock.Setup(s => s.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "test", PersonId = Guid.NewGuid() });

        _organisationClientMock.Setup(o => o.CreateOrganisationAsync(It.IsAny<NewOrganisation>()))
            .ReturnsAsync(GivenOrganisationClientModel());

        var model = GivenConsortiumCheckAnswerDetailModel();

        var actionResult = await model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConsortiumOverview");
    }

    private ConsortiumDetails DummyConsortiumDetails(string consortiumName = "Consortium 1",
        string consortiumEmailAddress = "test@example.com")
    {
        var consortiumDetails = new ConsortiumDetails
        {
            ConsortiumName = consortiumName,
            ConsortiumEmail = consortiumEmailAddress,
            PostalAddress = new Models.Address { AddressLine1 = "Address Line 1", TownOrCity = "London", Postcode = "SW1Y 5ED", CountryName = "United kindom", Country = "UK" }
        };

        return consortiumDetails;
    }

    private ConsortiumCheckAnswerModel GivenConsortiumCheckAnswerModel()
    {
        return new ConsortiumCheckAnswerModel(_sessionMock.Object, _organisationClientMock.Object);
    }
    private static CO.CDP.Organisation.WebApiClient.Organisation GivenOrganisationClientModel()
    {
        return new CO.CDP.Organisation.WebApiClient.Organisation(additionalIdentifiers: null, addresses: null, contactPoint: null, id: _consortiumId, identifier: null, name: "Test Org", type: CDP.Organisation.WebApiClient.OrganisationType.InformalConsortium, roles: [], details: new Details(approval: null, buyerInformation: null, pendingRoles: [], publicServiceMissionOrganization: null, scale: null, shelteredWorkshop: null, vcse: null));
    }
    private ConsortiumCheckAnswerModel GivenConsortiumCheckAnswerDetailModel()
    {
        var registrationDetails = DummyConsortiumDetails();

        _sessionMock.Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(registrationDetails);

        return new ConsortiumCheckAnswerModel(_sessionMock.Object, _organisationClientMock.Object);
    }
}