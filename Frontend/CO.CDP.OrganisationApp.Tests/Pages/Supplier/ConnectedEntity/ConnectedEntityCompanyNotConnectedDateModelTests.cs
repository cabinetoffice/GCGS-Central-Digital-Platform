using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCompanyNotConnectedDateModelTests
{
    private readonly Mock<ISession> _mockSession;
    private readonly ConnectedEntityCompanyNotConnectedDateModel _model;

    public ConnectedEntityCompanyNotConnectedDateModelTests()
    {
        _mockSession = new Mock<ISession>();
        _model = new ConnectedEntityCompanyNotConnectedDateModel(_mockSession.Object);
    }

    [Fact]
    public void OnGet_ShouldInitializeModelForOrganisation_WhenStateExists()
    {
        var state = new ConnectedEntityState
        {
            ConnectedEntityType = ConnectedEntityType.Organisation,
            OrganisationName = "Test Organisation",
            EndDate = new DateTimeOffset(new DateTime(2023, 5, 15), TimeSpan.Zero)
        };
        _mockSession.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Heading.Should().Be(string.Format(StaticTextResource.Supplier_ConnectedEntity_NotConnectedDate_Heading, state.OrganisationName));
        _model.Day.Should().Be("15");
        _model.Month.Should().Be("5");
        _model.Year.Should().Be("2023");
    }

    [Fact]
    public void OnGet_ShouldInitializeModelForTrustOrTrustee_WhenStateExists()
    {
        var state = new ConnectedEntityState
        {
            ConnectedEntityType = ConnectedEntityType.TrustOrTrustee,
            FirstName = "Trust_First_Name",
            LastName = "Trust_Last_Name",
            EndDate = new DateTimeOffset(new DateTime(2023, 5, 15), TimeSpan.Zero)
        };
        _mockSession.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Heading.Should().Be(string.Format(StaticTextResource.Supplier_ConnectedEntity_NotConnectedDate_Heading, string.Join(' ', [state.FirstName, state.LastName])));
        _model.Day.Should().Be("15");
        _model.Month.Should().Be("5");
        _model.Year.Should().Be("2023");
    }

    [Fact]
    public void OnGet_ShouldInitializeModelForIndividual_WhenStateExists()
    {
        var state = new ConnectedEntityState
        {
            ConnectedEntityType = ConnectedEntityType.Individual,
            FirstName = "Individual_First_Name",
            LastName = "Individual_Last_Name",
            EndDate = new DateTimeOffset(new DateTime(2023, 5, 15), TimeSpan.Zero)
        };
        _mockSession.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.Heading.Should().Be(string.Format(StaticTextResource.Supplier_ConnectedEntity_NotConnectedDate_Heading, string.Join(' ', [state.FirstName, state.LastName])));
        _model.Day.Should().Be("15");
        _model.Month.Should().Be("5");
        _model.Year.Should().Be("2023");
    }

    [Fact]
    public void OnPost_ShouldAddModelError_WhenDateIsInvalid()
    {
        _model.Day = "31";
        _model.Month = "2";
        _model.Year = "2023";

        var state = new ConnectedEntityState
        {
            ConnectedEntityType = ConnectedEntityType.Organisation
        };
        _mockSession.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).Returns(state);

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
        _model.ModelState.Should().ContainKey(nameof(_model.InsolvencyDate));
        _model.ModelState[nameof(_model.InsolvencyDate)]!.Errors[0].ErrorMessage.Should().Be(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCompanyInsolvencyDate_InvalidDate);
    }

    [Fact]
    public void OnPost_ShouldRedirectToCheckAnswersPage_WhenValidDateIsProvided()
    {
        _model.Day = "15";
        _model.Month = "5";
        _model.Year = "2023";

        var state = new ConnectedEntityState
        {
            ConnectedEntityType = ConnectedEntityType.Organisation
        };
        _mockSession.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).Returns(state);

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        Assert.Equal("ConnectedEntityCheckAnswersOrganisation", ((RedirectToPageResult)result).PageName);
    }
}