using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityPscDetailsTests
{
    private readonly ConnectedEntityPscDetailsModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityPscDetailsTests()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityPscDetailsModel(_sessionMock.Object);

        _model.Id = _organisationId;
        _model.Day = "1";
        _model.Month = "1";
        _model.Year = "1990";
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntitySupplierCompanyQuestionPage_WhenModelStateIsInvalid()
    {
        ConnectedEntityState? state = null;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");
    }

    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        var state = DummyConnectedPersonDetails();
        _model.ConnectedEntityId = _entityId;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.FirstName.Should().Be("John");
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntityQuestion_WhenSessionStateIsNull()
    {
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns((ConnectedEntityState?)null);

        var result = _model.OnGet();

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedEntitySupplierCompanyQuestion");
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
    [InlineData("31", "02", "2021", "Date of birth must be a real date")]
    [InlineData("01", "01", "2100", "Date of birth must be today or in the past")]
    public void OnPost_AddsModelError_WhenDateIsInvalid(string day, string month, string year, string expectedError)
    {
        SetDateFields(day, month, year);
        var state = DummyConnectedPersonDetails();
        _model.FirstName = state.FirstName;
        _model.LastName = state.LastName;
        _model.Day = day;
        _model.Month = month;
        _model.Year = year;
        _model.Nationality = state.Nationality;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);
        var result = _model.OnPost();

        _model.ModelState.ContainsKey(nameof(_model.DateOfBirth)).Should().BeTrue();
        _model.ModelState[nameof(_model.DateOfBirth)]?.Errors[0].ErrorMessage.Should().Be(expectedError);
        result.Should().BeOfType<PageResult>();
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

    [Fact]
    public void OnPost_ShouldRedirectToConnectedEntityAddressPage()
    {
        var state = DummyConnectedPersonDetails();
        _model.FirstName = state.FirstName;
        _model.LastName = state.LastName;
        _model.DateOfBirth = state.DateOfBirth.ToString();
        _model.Nationality = state.Nationality;
        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("ConnectedEntityAddress");
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenDateIsInvalid()
    {
        SetDateFields("31", "2", "2023");
        var state = DummyConnectedPersonDetails();
        _model.FirstName = state.FirstName;
        _model.LastName = state.LastName;
        _model.DateOfBirth = state.DateOfBirth.ToString();
        _model.Nationality = state.Nationality;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);


        var result = _model.OnPost();

        _model.ModelState.IsValid.Should().BeFalse();
        _model.ModelState.Should().ContainKey(nameof(_model.DateOfBirth));
        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        var state = DummyConnectedPersonDetails();
        _model.FirstName = state.FirstName;
        _model.LastName = state.LastName;
        _model.DateOfBirth = state.DateOfBirth.ToString();
        _model.Day = state.DateOfBirth!.Value.Day.ToString();
        _model.Month = state.DateOfBirth!.Value.Month.ToString();
        _model.Year = state.DateOfBirth!.Value.Year.ToString();
        _model.Nationality = state.Nationality;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(s => s.Set(Session.ConnectedPersonKey, It.Is<ConnectedEntityState>(st => st.ConnectedEntityType == Constants.ConnectedEntityType.Individual)), Times.Once);

    }

    private ConnectedEntityState DummyConnectedPersonDetails()
    {
        var connectedPersonDetails = new ConnectedEntityState
        {
            ConnectedEntityId = _entityId,
            SupplierHasCompanyHouseNumber = true,
            SupplierOrganisationId = _organisationId,
            ConnectedEntityType = Constants.ConnectedEntityType.Individual,
            ConnectedEntityIndividualAndTrustCategoryType = Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual,
            FirstName = "John",
            LastName = "Doe",
            Nationality = "British",
            DateOfBirth = new DateTimeOffset(new DateTime(1990, 1, 1)),
        };

        return connectedPersonDetails;
    }
    private void SetDateFields(string day, string month, string year)
    {
        _model.Day = day;
        _model.Month = month;
        _model.Year = year;
    }
}