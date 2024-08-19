using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityCompanyInsolvencyDateTest
{
    private readonly ConnectedEntityCompanyInsolvencyDateModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityCompanyInsolvencyDateTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityCompanyInsolvencyDateModel(_sessionMock.Object);
        _model.Id = _organisationId;
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _sessionMock
           .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
           .Returns((ConnectedEntityState?)null);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    public static IEnumerable<object[]> Guids
    {
        get
        {
            var v = (Guid?)null;
            yield return new object[] { v!, "ConnectedEntitySupplierCompanyQuestion" };
            yield return new object[] { Guid.NewGuid(), "ConnectedEntityCheckAnswersIndividualOrTrust" };
        }
    }

    [Theory, MemberData(nameof(Guids))]
    public void OnGet_ShouldRedirectToExpectedRedirectPage_WhenModelStateIsInvalid
        (Guid? connectedEntityId, string expectedRedirectPage)
    {
        ConnectedEntityState? state = null;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);
        _model.ConnectedEntityId = connectedEntityId;

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be(expectedRedirectPage);
    }

    [Fact]
    public void OnGet_ShouldPopulateFields_WhenDateInsolvencyIsPresent()
    {
        var state = DummyConnectedPersonDetails();
        state.InsolvencyDate = new DateTime(2010, 6, 11);

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        _model.Day.Should().Be("11");
        _model.Month.Should().Be("6");
        _model.Year.Should().Be("2010");
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalidDay()
    {
        _model.Day = "40";
        _model.Month = "14";
        _model.Year = "2023";
        _model.ModelState.AddModelError("Day", "Invalid day");

        var state = DummyConnectedPersonDetails();

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Theory]
    [InlineData("ConnectedEntityCheckAnswersOrganisation")]
    public void OnPost_ShouldRedirectToExpectedPage_WhenModelStateIsValid(string expectedRedirectPage)
    {
        SetValidDate();

        var state = DummyConnectedPersonDetails();

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedRedirectPage);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenDateIsInvalid()
    {
        SetDateFields("31", "2", "2023");
        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        _model.ModelState.Should().ContainKey(nameof(_model.InsolvencyDate));
        result.Should().BeOfType<PageResult>();
    }

    [Theory]
    [InlineData("31", "02", "2021", "Date of insolvency must be a real date")]
    [InlineData("01", "01", "2100", "Date of insolvency must be today or in the past")]
    public void OnPost_AddsModelError_WhenDateIsInvalid(string day, string month, string year, string expectedError)
    {
        SetDateFields(day, month, year);
        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);
        var result = _model.OnPost();

        _model.ModelState.ContainsKey(nameof(_model.InsolvencyDate)).Should().BeTrue();
        _model.ModelState[nameof(_model.InsolvencyDate)]?.Errors[0].ErrorMessage.Should().Be(expectedError);
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        SetValidDate();

        var state = DummyConnectedPersonDetails();

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(v => v.Set(Session.ConnectedPersonKey,
            It.Is<ConnectedEntityState>(rd =>
                rd.ControlConditions!.Contains(Constants.ConnectedEntityControlCondition.OwnsShares)
            )), Times.Once);
    }

    [Theory]
    [InlineData(true, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityCheckAnswersOrganisation", "company-question")]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityCheckAnswersOrganisation", "company-question")]
    [InlineData(false, Constants.ConnectedEntityType.Organisation, ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver, "ConnectedEntityCheckAnswersOrganisation", "overseas-company-question", false)]
    public void OnPost_ShouldSetBackPageLink_ToExpectedPage(
            bool yesJourney,
            Constants.ConnectedEntityType connectedEntityType,
            Constants.ConnectedEntityOrganisationCategoryType organisationCategoryType,
            string expectedRedirectPage, string expectedBackPageName, bool hasCompanyHouseNumber = true)
    {
        SetValidDate();

        var state = DummyConnectedPersonDetails();
        state.SupplierHasCompanyHouseNumber = yesJourney;
        state.ConnectedEntityType = connectedEntityType;
        state.ConnectedEntityOrganisationCategoryType = organisationCategoryType;
        state.HasCompaniesHouseNumber = hasCompanyHouseNumber;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnPost();
                
        var pageResult = result.Should().BeOfType<RedirectToPageResult>();

        pageResult.Which.PageName.Should().Be(expectedRedirectPage);

        _model.BackPageLink.Should().Be(expectedBackPageName);
    }

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Organisation,
            OrganisationName = "Org_name",
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678",
            ControlConditions = [Constants.ConnectedEntityControlCondition.OwnsShares],
            RegistrationDate = new DateTimeOffset(2011, 7, 15, 0, 0, 0, TimeSpan.FromHours(0)),
            InsolvencyDate = new DateTimeOffset(2010, 6, 11, 0, 0, 0, TimeSpan.FromHours(0)),
        };

        return connectedPersonDetails;
    }

    private void SetDateFields(string day, string month, string year)
    {
        _model.Day = day;
        _model.Month = month;
        _model.Year = year;
    }

    private void SetValidDate()
    {
        _model.Day = "15";
        _model.Month = "6";
        _model.Year = "2023";
    }
    private void SetFutureDate()
    {
        var futureDate = DateTime.Now.AddDays(1);
        _model.Day = futureDate.Day.ToString();
        _model.Month = futureDate.Month.ToString();
        _model.Year = futureDate.Year.ToString();
    }
}