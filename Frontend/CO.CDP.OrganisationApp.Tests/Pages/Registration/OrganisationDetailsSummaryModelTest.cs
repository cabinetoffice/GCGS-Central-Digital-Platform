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

public class OrganisationDetailsSummaryModelTest
{
    private readonly Mock<ISession> sessionMock;

    public OrganisationDetailsSummaryModelTest()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_ValidSession_ReturnsRegistrationDetails()
    {        
        var model = GivenOrganisationDetailModel();

        RegistrationDetails registrationDetails = DummyRegistrationDetails();

        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(registrationDetails);

        model.OnGet();
        model.RegistrationDetailModel.As<RegistrationDetails>().Should().NotBeNull();
    }

    private RegistrationDetails DummyRegistrationDetails()
    {
        var registrationDetails = new RegistrationDetails
        {
            OrganisationName = "TestOrg",
            OrganisationType = "TestType",
            OrganisationEmailAddress = "test@example.com",
            OrganisationTelephoneNumber = "1234567890"
        };

        return registrationDetails;
    }

    [Fact]
    public void OnGet_InvalidSession_ThrowsException()
    {
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey)).Returns(value: null);

        var model = GivenOrganisationDetailModel();

        Action action = () => model.OnGet();
        action.Should().Throw<Exception>().WithMessage("Shoudn't be here");
    }

    private OrganisationDetailsSummaryModel GivenOrganisationDetailModel()
    {
        return new OrganisationDetailsSummaryModel(sessionMock.Object);
    }
}
