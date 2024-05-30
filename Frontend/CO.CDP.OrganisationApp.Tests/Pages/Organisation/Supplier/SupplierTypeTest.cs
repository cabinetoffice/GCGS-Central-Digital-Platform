using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Organisation.Supplier;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation.Supplier;

public class SupplierTypeTest
{
    private readonly Mock<ISession> sessionMock;

    public SupplierTypeTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_WhenOrganisationDetailsNotInSession_ShouldThrowException()
    {
        var model = GivenSupplierTypeModel();

        Action act = () => model.OnGet(Guid.NewGuid());

        act.Should().Throw<Exception>().WithMessage(ErrorMessagesList.SessionNotFound);
    }

    [Fact]
    public void OnGet_WhenSupplierTypeSetInSession_ShouldPopulatePageModel()
    {
        var model = GivenSupplierTypeModel();
        var id = Guid.NewGuid();
        sessionMock.Setup(s => s.Get<OrganisationDetails>(Session.OrganisationDetailsKey))
            .Returns(new OrganisationDetails
            {
                OrganisationId = id,
                SupplierType = SupplierType.Organisation,
            });

        model.OnGet(id);

        model.SupplierType.Should().Be(SupplierType.Organisation);
    }

    [Fact]
    public void OnPost_WhenInValidModel_ShouldReturnSamePage()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("error", "some error");
        var actionContext = new ActionContext(new DefaultHttpContext(),
            new RouteData(), new PageActionDescriptor(), modelState);
        var pageContext = new PageContext(actionContext);

        var model = GivenSupplierTypeModel();
        model.PageContext = pageContext;

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_WhenOrganisationDetailsNotInSession_ShouldThrowException()
    {
        var model = GivenSupplierTypeModel();

        Action act = () => model.OnPost();

        act.Should().Throw<Exception>().WithMessage(ErrorMessagesList.SessionNotFound);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldSetOrganisationTypeInSession()
    {
        var model = GivenSupplierTypeModel();
        model.SupplierType = SupplierType.Organisation;

        sessionMock.Setup(s => s.Get<OrganisationDetails>(Session.OrganisationDetailsKey))
            .Returns(new OrganisationDetails { OrganisationId = Guid.NewGuid(), SupplierType = SupplierType.Organisation });

        var results = model.OnPost();

        sessionMock.Verify(v => v.Set(Session.OrganisationDetailsKey,
            It.Is<OrganisationDetails>(rd =>
                rd.SupplierType == SupplierType.Organisation
            )), Times.Once);
    }

    [Fact]
    public void OnPost_WhenValidModel_ShouldRedirectToBasicInformationPage()
    {
        var model = GivenSupplierTypeModel();

        sessionMock.Setup(s => s.Get<OrganisationDetails>(Session.OrganisationDetailsKey))
            .Returns(new OrganisationDetails { OrganisationId = Guid.NewGuid(), SupplierType = SupplierType.Organisation });

        var actionResult = model.OnPost();

        actionResult.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("/Organisation/BasicInformation");
    }

    private SupplierTypeModel GivenSupplierTypeModel()
    {
        return new SupplierTypeModel(sessionMock.Object);
    }
}