using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class JoinOrganisationSuccessModelTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly JoinOrganisationSuccessModel _joinOrganisationSuccessModel;
    private readonly string _organisationName = "Test Org";
    private readonly Guid _organisationId = Guid.NewGuid();

    public JoinOrganisationSuccessModelTests()
    {
        _sessionMock = new Mock<ISession>();
        _joinOrganisationSuccessModel = new JoinOrganisationSuccessModel(_sessionMock.Object);
    }

    [Fact]
    public void OnGet_ValidOrganisationId_ReturnsPageResult()
    {
        JoinOrganisationRequestState? state = GetRequestState();

        _sessionMock
            .Setup(s => s.Get<JoinOrganisationRequestState>(Session.JoinOrganisationRequest))
            .Returns(state);

        _joinOrganisationSuccessModel.Id = _organisationId;

        var result = _joinOrganisationSuccessModel.OnGet();
                
        result.Should().BeOfType<PageResult>();
        _joinOrganisationSuccessModel.OrganisationName.Should().Be(_organisationName);
        _sessionMock.Verify(s => s.Remove(Session.JoinOrganisationRequest), Times.Once);
    }
        [Fact]
    public void OnGet_WhenSessionRequestDoesNotMatch_ShouldRedirectToOrganisationSelection()
    {
        var organisationId = Guid.NewGuid();

        var jor = new JoinOrganisationRequestState { OrganisationId = Guid.NewGuid(), OrganisationName = "Another Org" };

        _sessionMock
            .Setup(s => s.Get<JoinOrganisationRequestState>(Session.JoinOrganisationRequest))
            .Returns(jor);
        _joinOrganisationSuccessModel.Id = organisationId;

        var result = _joinOrganisationSuccessModel.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/organisation-selection");
        _sessionMock.Verify(s => s.Remove(Session.JoinOrganisationRequest), Times.Once);
    }

    [Fact]
    public void OnGet_WhenSessionRequestIsNull_ShouldRedirectToOrganisationSelection()
    {
        _sessionMock
            .Setup(s => s.Get<JoinOrganisationRequestState>(Session.JoinOrganisationRequest))
            .Returns((JoinOrganisationRequestState?)null);
        _joinOrganisationSuccessModel.Id = Guid.NewGuid();

        var result = _joinOrganisationSuccessModel.OnGet();

        result.Should().BeOfType<RedirectResult>()
            .Which.Url.Should().Be("/organisation-selection");
        _sessionMock.Verify(s => s.Remove(Session.JoinOrganisationRequest), Times.Once);
    }

    private JoinOrganisationRequestState GetRequestState()
    {
        return new JoinOrganisationRequestState
        {
            OrganisationId = _organisationId,
            OrganisationName = _organisationName
        };
    }
}
