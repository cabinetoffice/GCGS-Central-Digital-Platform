using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class BuyerOrganisationTypeTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly BuyerOrganisationTypeModel _model;
    private readonly RegistrationDetails _registrationDetails;

    public BuyerOrganisationTypeTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new BuyerOrganisationTypeModel(_sessionMock.Object);
        _registrationDetails = new RegistrationDetails();
        _sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
                    .Returns(_registrationDetails);
    }


    [Fact]
    public void OnGet_SetsBuyerOrganisationTypeAndOtherValueToNull_WhenBuyerOrganisationTypeIsNullOrEmpty()
    {
        _registrationDetails.BuyerOrganisationType = null;

        var result = _model.OnGet();

        _model.BuyerOrganisationType.Should().BeNull();
        _model.OtherValue.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_RedirectsToSummary_WhenRedirectToSummaryIsTrue()
    {
        _model.BuyerOrganisationType = "CentralGovernment";
        _model.RedirectToSummary = true;
        _model.ModelState.Clear();

        var result = _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnPost_RedirectsToBuyerDevolvedRegulation_WhenRedirectToSummaryIsFalse()
    {
        _model.BuyerOrganisationType = "CentralGovernment";
        _model.RedirectToSummary = false;
        _model.ModelState.Clear();

        var result = _model.OnPost();

        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("BuyerDevolvedRegulation");
    }

    [Fact]
    public void OnPost_ReturnsPage_WhenModelStateIsInvalid()
    {
        _model.ModelState.AddModelError("BuyerOrganisationType", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

}