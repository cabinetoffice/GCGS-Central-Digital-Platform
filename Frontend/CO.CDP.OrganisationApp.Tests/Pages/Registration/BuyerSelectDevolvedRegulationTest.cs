using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class BuyerSelectDevolvedRegulationTest
{
    private readonly Mock<ISession> sessionMock;

    public BuyerSelectDevolvedRegulationTest()
    {
        sessionMock = new Mock<ISession>();
        sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
            .Returns(new UserDetails { UserUrn = "urn:test" });
    }

    [Fact]
    public void OnGet_WhenRegulationSetInSession_ShouldPopulatePageModel()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                OrganisationType = OrganisationType.Buyer,
                Devolved = true,
                Regulations = [DevolvedRegulation.NorthernIreland]
            });

        var model = GivenBuyerSelectDevolvedRegulationModel();

        model.OnGet();

        model.Regulations.Should().Contain(DevolvedRegulation.NorthernIreland);
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenBuyerSelectDevolvedRegulationModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetRegulationsInSession()
    {
        var model = GivenBuyerSelectDevolvedRegulationModel();
        model.Regulations = [DevolvedRegulation.NorthernIreland];

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { Devolved = true, Regulations = [DevolvedRegulation.NorthernIreland] });

        var results = model.OnPost();

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey,
            It.Is<RegistrationDetails>(rd =>
                rd.Regulations!.Contains(DevolvedRegulation.NorthernIreland)
            )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToOrganisationDetailsSummaryPage()
    {
        var model = GivenBuyerSelectDevolvedRegulationModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { Devolved = true, Regulations = [DevolvedRegulation.NorthernIreland] });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    private BuyerSelectDevolvedRegulationModel GivenBuyerSelectDevolvedRegulationModel()
    {
        return new BuyerSelectDevolvedRegulationModel(sessionMock.Object) { Regulations = [] };
    }
}