using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityDirectorResidencyTest
{
    private readonly ConnectedEntityDirectorResidencyModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityDirectorResidencyTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityDirectorResidencyModel(_sessionMock.Object);
        _model.Id = _organisationId;
        _model.DirectorLocation = "British";
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
    public void OnGet_ShouldReturnPageResult()
    {
        var state = DummyConnectedPersonDetails();
        _model.ConnectedEntityId = _entityId;
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
        _model.DirectorLocation.Should().Be("British");
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
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _sessionMock
           .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
           .Returns((ConnectedEntityState?)null);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
    }

    [Theory]
    [InlineData("ConnectedEntityAddress", false)]
    [InlineData("ConnectedEntityCheckAnswersIndividualOrTrust", true)]
    public void OnPost_ShouldRedirectToExpectedPagePage(string expectedPage, bool redirectToCheckYourAnswer)
    {
        var state = DummyConnectedPersonDetails();

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);
        _model.RedirectToCheckYourAnswer = redirectToCheckYourAnswer;

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be(expectedPage);
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        var state = DummyConnectedPersonDetails();

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
            ConnectedEntityIndividualAndTrustCategoryType = Constants.ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust,
            FirstName = "John",
            LastName = "Doe",
            DirectorLocation = "British",
            DateOfBirth = new DateTimeOffset(new DateTime(1990, 1, 1)),
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678",
            RegisterName = "reg_name"
        };

        return connectedPersonDetails;
    }
}