using CO.CDP.OrganisationApp.Pages.Consortium;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumNameTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly ConsortiumNameModel _model;

    public ConsortiumNameTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConsortiumNameModel(_sessionMock.Object);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _sessionMock
           .Setup(s => s.Get<ConsortiumState>(Session.ConsortiumKey))
           .Returns((ConsortiumState?)null);

        _model.ModelState.AddModelError("Error", "Model state is invalid");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldUpdateSessionState_WhenModelStateIsValid()
    {
        var state = DummyConsortiumDetails();

        _model.ConsortiumName = "consortium_name";

        _sessionMock
            .Setup(s => s.Get<ConsortiumState>(Session.ConsortiumKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(s => s.Set(Session.ConsortiumKey, It.Is<ConsortiumState>(st => st.ConstortiumName == "consortium_name")), Times.Once);
    }

    [Fact]
    public void OnPost_ShouldRedirectToConsortiumAddressPage_WhenModelStateIsValid()
    {
        var state = DummyConsortiumDetails();

        _model.ConsortiumName = "consortium_name";

        _sessionMock
            .Setup(s => s.Get<ConsortiumState>(Session.ConsortiumKey))
            .Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("ConsortiumAddress");
    }

    private ConsortiumState DummyConsortiumDetails()
    {
        var consortiumState = new ConsortiumState
        {
            ConstortiumName = "consortium_name"
        };

        return consortiumState;
    }
}