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

public class OrganisationTypeTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationTypeTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_WhenOrganisationTypeSetInSession_ShouldPopulatePageModel()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails
            {
                OrganisationType = OrganisationType.Supplier
            });

        var model = GivenOrganisationTypeModel();

        model.OnGet();

        model.OrganisationType.Should().Be(OrganisationType.Supplier);
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenOrganisationTypeModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetOrganisationTypeInSession()
    {
        var model = GivenOrganisationTypeModel();
        model.OrganisationType = OrganisationType.Supplier;

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails());

        var results = model.OnPost();

        sessionMock.Verify(v => v.Set(Session.RegistrationDetailsKey,
            It.Is<RegistrationDetails>(rd =>
                rd.OrganisationType == OrganisationType.Supplier
            )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToCompanyHouseNumberQuestionPage()
    {
        var model = GivenOrganisationTypeModel();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails());

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("CompanyHouseNumberQuestion");
    }

    [Fact]
    public void OnPost_WhenValidModelAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = GivenOrganisationTypeModel();
        model.RedirectToSummary = true;

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(new RegistrationDetails());

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("OrganisationDetailsSummary");
    }

    private OrganisationTypeModel GivenOrganisationTypeModel()
    {
        return new OrganisationTypeModel(sessionMock.Object);
    }
}