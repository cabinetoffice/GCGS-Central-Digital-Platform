using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Registration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;
public class CompanyHouseNumberQuestionTests
{
    private readonly Mock<ISession> sessionMock;

    public CompanyHouseNumberQuestionTests()
    {
        sessionMock = new Mock<ISession>();
    }

    [Fact]
    public void OnGet_WheEmptyModel_ShouldThrowException()
    {
        var model = new CompanyHouseNumberQuestionModel(sessionMock.Object);
        Action action = () => model.OnGet();
        action.Should().Throw<Exception>().WithMessage("Session not found");
    }

    [Theory]
    [InlineData(null)]
    [InlineData(false)]
    public void OnPost_WhenOptionIsNullOrEmpty_ShouldReturnPageWithModelStateError(bool? hasNumber)
    {

        var model = new CompanyHouseNumberQuestionModel(sessionMock.Object)
        {
            HasCompaniesHouseNumber = hasNumber
        };
        model.ModelState.AddModelError("Question", "Please select an option");

        var result = model.OnPost();

        result.Should().BeOfType<PageResult>();
        model.ModelState.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OnPost_WhenModelStateIsValid_ShouldRedirectToOrganisationIdentification()
    {
        var model = new CompanyHouseNumberQuestionModel(sessionMock.Object)
        {
            HasCompaniesHouseNumber = false,
            CompaniesHouseNumber = "12345678"
        };

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        var result = model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationIdentification");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValid_And_CompaniesNumber_Provided_ShouldRedirectToOrganisationName()
    {
        var model = new CompanyHouseNumberQuestionModel(sessionMock.Object)
        {
            HasCompaniesHouseNumber = true,
            CompaniesHouseNumber = "12345678"
        };

        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        var result = model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationName");
    }

    [Fact]
    public void OnPost_WhenModelStateIsValidAndRedirectToSummary_ShouldRedirectToOrganisationDetailSummaryPage()
    {
        var model = new CompanyHouseNumberQuestionModel(sessionMock.Object)
        {
            HasCompaniesHouseNumber = true,
            RedirectToSummary = true,
            CompaniesHouseNumber = "123456"
        };
        GivenRegistrationIsInProgress(model.HasCompaniesHouseNumber, model.CompaniesHouseNumber);

        var result = model.OnPost();
        result.Should().BeOfType<RedirectToPageResult>();
        (result as RedirectToPageResult)?.PageName.Should().Be("OrganisationDetailsSummary");
    }

    private void GivenRegistrationIsInProgress(bool? hasNumber, string companyHouseNumber)
    {
        RegistrationDetails registrationDetails = DummyRegistrationDetails(hasNumber, companyHouseNumber);
        sessionMock.Setup(s => s.Get<RegistrationDetails>(Session.RegistrationDetailsKey))
            .Returns(registrationDetails);
    }

    private static RegistrationDetails DummyRegistrationDetails(bool? hasNumber, string companyHouseNumber)
    {
        var registrationDetails = new RegistrationDetails
        {
            UserUrn = "urn:fdc:gov.uk:2022:37d8856672e84a57ae9c86b27b226225",
            OrganisationScheme = "CCEW",
            OrganisationIdentificationNumber = companyHouseNumber,
            OrganisationHasCompaniesHouseNumber = hasNumber
        };

        return registrationDetails;
    }
}
