using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Consortium;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Consortium;

public class ConsortiumNameModelTest
{
    private readonly Mock<ISession> _sessionMock;
    private readonly ConsortiumNameModel _model;

    public ConsortiumNameModelTest()
    {
        _sessionMock = new Mock<ISession>();
        _model = new ConsortiumNameModel(_sessionMock.Object);
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _sessionMock
           .Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
           .Returns((ConsortiumDetails?)null);

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
            .Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(state);

        _model.OnPost();

        _sessionMock.Verify(s => s.Set(Session.ConsortiumKey, It.Is<ConsortiumDetails>(st => st.ConsortiumName == "consortium_name")), Times.Once);
    }       

    [Fact]
    public void OnPost_ShouldRedirectToConsortiumAddressPage_WhenModelStateIsValid()
    {
        var state = DummyConsortiumDetails();

        _model.ConsortiumName = "consortium_name";

        _sessionMock
            .Setup(s => s.Get<ConsortiumDetails>(Session.ConsortiumKey))
            .Returns(state);

        var result = _model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        redirectToPageResult.PageName.Should().Be("ConsortiumAddress");
    }

    private ConsortiumDetails DummyConsortiumDetails()
    {
        var consortiumState = new ConsortiumDetails
        {
            ConsortiumName = "consortium_name"
        };

        return consortiumState;
    }
}