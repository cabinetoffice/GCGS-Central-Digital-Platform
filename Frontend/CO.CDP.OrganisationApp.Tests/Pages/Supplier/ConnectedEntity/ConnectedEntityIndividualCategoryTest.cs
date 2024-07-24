using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedEntityIndividualCategoryTest
{
    private readonly ConnectedEntityIndividualCategoryModel _model;
    private readonly Mock<ISession> _sessionMock;
    private readonly Guid _organisationId = Guid.NewGuid();
    private readonly Guid _entityId = Guid.NewGuid();

    public ConnectedEntityIndividualCategoryTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConnectedEntityIndividualCategoryModel(_sessionMock.Object);
        _model.Id = Guid.NewGuid();
    }

    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(DummyConnectedPersonDetails());

        var result = _model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnGet_ShouldRedirectToConnectedEntitySupplierCompanyQuestion_WhenSessionStateIsNull()
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

        result.Should().BeOfType<PageResult>();
    }


    [Theory]    
    [InlineData(Constants.ConnectedEntityType.Individual, "ConnectedEntityPscDetails")]
    public void OnPost_ShouldRedirectToConnectedEntityPscDetailsPage(Constants.ConnectedEntityType connectedEntityType, string expectedRedirectPage)
    {
        var state = DummyConnectedPersonDetails();
        state.ConnectedEntityType = connectedEntityType;
        _model.ConnectedEntityCategory = Constants.ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual;

        _sessionMock.Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey)).
            Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be(expectedRedirectPage);
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        var state = DummyConnectedPersonDetails();

        state.ConnectedEntityType = Constants.ConnectedEntityType.Organisation;

        _sessionMock
            .Setup(s => s.Get<ConnectedEntityState>(Session.ConnectedPersonKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(s => s.Set(Session.ConnectedPersonKey, It.Is<ConnectedEntityState>(st => st.ConnectedEntityType == Constants.ConnectedEntityType.Organisation)), Times.Once);
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
        };

        return connectedPersonDetails;
    }
}