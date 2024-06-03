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

public class BuyerDevolvedRegulationTest
{
    private readonly Mock<ISession> sessionMock;

    public BuyerDevolvedRegulationTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_WhenDevolvedSetInSession_ShouldPopulatePageModel()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                OrganisationType = OrganisationType.Buyer,
                Devolved = true
            });

        var model = GivenBuyerDevolvedRegulationModel();

        model.OnGet();

        model.Devolved.Should().BeTrue();
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenBuyerDevolvedRegulationModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetDevolvedInSession()
    {
        var model = GivenBuyerDevolvedRegulationModel();
        model.Devolved = true;

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { Devolved = true });

        var results = model.OnPost();

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey,
            It.Is<RegistrationDetails>(rd =>
                rd.Devolved == true
            )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModelWithYes_ShouldRedirectToBuyerSelectDevolvedRegulationPage()
    {
        var model = GivenBuyerDevolvedRegulationModel();
        model.Devolved = true;

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { Devolved = true });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("BuyerSelectDevolvedRegulation");
    }

    [Fact]
    public void OnPost_WhenValidModelWithNo_ShouldRedirectToOrganisationSelectionPage()
    {
        var model = GivenBuyerDevolvedRegulationModel();
        model.Devolved = false;

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { Devolved = false });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/OrganisationSelection");
    }

    private BuyerDevolvedRegulationModel GivenBuyerDevolvedRegulationModel()
    {
        return new BuyerDevolvedRegulationModel(sessionMock.Object);
    }
}