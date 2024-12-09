using CO.CDP.OrganisationApp.Pages.Registration;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using Amazon.S3;
using CO.CDP.OrganisationApp.Models;


namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationInternationalIdentificationCountryTests
{
    private readonly Mock<ISession> _sessionMock;
    private readonly OrganisationInternationalIdentificationCountryModel _model;

    public OrganisationInternationalIdentificationCountryTests()
    {
        _sessionMock = new Mock<ISession>();
        _model = new OrganisationInternationalIdentificationCountryModel(_sessionMock.Object);

        _sessionMock.Setup(session => session.Get<UserDetails>(Session.UserDetailsKey))
           .Returns(new UserDetails { UserUrn = "urn:test" });

    }

    [Fact]
    public void OnGet_ShouldSetCountryFromRegistrationDetails()
    {
        var expectedCountry = "France";
        _model.RegistrationDetails.OrganisationIdentificationCountry = expectedCountry;

        _model.OnGet();

        _model.Country.Should().Be(expectedCountry);
    }

    [Fact]
    public void OnPost_InvalidModelState_ShouldReturnPageResult()
    {

        _model.Country = null;
        _model.ModelState.AddModelError("Country", "Required");

        var result = _model.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ValidModelState_ShouldUpdateRegistrationDetailsAndRedirect()
    {
        var country = "France";
        _model.Country = country;

        var result = _model.OnPost();

        _model.RegistrationDetails.OrganisationIdentificationCountry.Should().Be(country);

        _sessionMock.Verify(
        session => session.Set(Session.RegistrationDetailsKey, It.Is<RegistrationDetails>(rd =>
            rd.OrganisationIdentificationCountry == country)),
        Times.Once);

        result.Should().BeOfType<RedirectToPageResult>()
              .Which.PageName.Should().Be("OrganisationInternationalIdentification");
    }
}
