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

public class BuyerOrganisationTypeTest
{
    private readonly Mock<ISession> sessionMock;

    public BuyerOrganisationTypeTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_WhenOrganisationTypeSetInSession_ShouldPopulatePageModel()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                OrganisationType = OrganisationType.Buyer,
                BuyerOrganisationType = "type1"
            });

        var model = GivenBuyerOrganisationTypeModel();

        model.OnGet();

        model.BuyerOrganisationType.Should().Be("type1");
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenBuyerOrganisationTypeModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetOrganisationTypeInSession()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { OrganisationType = OrganisationType.Buyer });

        var model = GivenBuyerOrganisationTypeModel();
        model.BuyerOrganisationType = "type1";

        var results = model.OnPost();

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey,
            It.Is<RegistrationDetails>(rd =>
                rd.OrganisationType == OrganisationType.Buyer
            )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToBuyerDevolvedRegulationsPage()
    {
        var model = GivenBuyerOrganisationTypeModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails { OrganisationType = OrganisationType.Buyer });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("BuyerDevolvedRegulation");
    }

    private BuyerOrganisationTypeModel GivenBuyerOrganisationTypeModel()
    {
        return new BuyerOrganisationTypeModel(sessionMock.Object);
    }
}