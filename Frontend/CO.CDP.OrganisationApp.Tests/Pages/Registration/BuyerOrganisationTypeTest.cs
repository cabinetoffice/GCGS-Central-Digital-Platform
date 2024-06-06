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
        // Arrange
        _registrationDetails.BuyerOrganisationType = null;

        // Act
        var result = _model.OnGet();

        // Assert
        _model.BuyerOrganisationType.Should().BeNull();
        _model.OtherValue.Should().BeNull();
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_RedirectsToSummary_WhenRedirectToSummaryIsTrue()
    {
        // Arrange
        _model.BuyerOrganisationType = "CentralGovernment";
        _model.RedirectToSummary = true;
        _model.ModelState.Clear();

        // Act
        var result = _model.OnPost();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("OrganisationDetailsSummary");
    }

    [Fact]
    public void OnPost_RedirectsToBuyerDevolvedRegulation_WhenRedirectToSummaryIsFalse()
    {
        // Arrange
        _model.BuyerOrganisationType = "CentralGovernment";
        _model.RedirectToSummary = false;
        _model.ModelState.Clear();

        // Act
        var result = _model.OnPost();

        // Assert
        var redirectResult = result.Should().BeOfType<RedirectToPageResult>().Subject;
        redirectResult.PageName.Should().Be("BuyerDevolvedRegulation");
    }

    [Fact]
    public void OnPost_ReturnsPage_WhenModelStateIsInvalid()
    {
        // Arrange
        _model.ModelState.AddModelError("BuyerOrganisationType", "Required");

        // Act
        var result = _model.OnPost();

        // Assert
        result.Should().BeOfType<PageResult>();
    }

}