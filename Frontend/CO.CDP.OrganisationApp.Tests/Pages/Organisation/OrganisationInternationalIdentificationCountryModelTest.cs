using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Organisation;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System.ComponentModel.DataAnnotations;
using WebApiClientOrganisation = CO.CDP.Organisation.WebApiClient.Organisation;

namespace CO.CDP.OrganisationApp.Tests.Pages.Organisation;

public class OrganisationInternationalIdentificationCountryModelTest
{
    private readonly OrganisationInternationalIdentificationCountryModel _pageModel;

    public OrganisationInternationalIdentificationCountryModelTest()
    {
        _pageModel = new OrganisationInternationalIdentificationCountryModel()
        {
            Id = Guid.NewGuid(),
            PageContext = new PageContext { HttpContext = new DefaultHttpContext() }
        };
    }

    [Fact]
    public void OnPost_ShouldReturnPage_WhenModelStateIsInvalid()
    {
        _pageModel.ModelState.AddModelError("Country", "Required");

        var result = _pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void Country_ShouldHaveRequiredValidation()
    {
        var pageModel = new OrganisationInternationalIdentificationCountryModel();
        var context = new ValidationContext(pageModel) { MemberName = "Country" };
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateProperty(pageModel.Country, context, results);

        isValid.Should().BeFalse();
        results.Should().ContainSingle()
              .Which.ErrorMessage.Should().Be("Select a country");
    }



    [Fact]
    public void OnGet_ShouldSetCountryFromQuery()
    {
        _pageModel.HttpContext.Request.QueryString = new QueryString("?country=USA");

        _pageModel.OnGet();

        _pageModel.Country.Should().Be("USA");
    }

    [Fact]
    public void OnPost_ShouldRedirectToPage_WhenModelStateIsValid()
    {
        _pageModel.Country = "USA";
        _pageModel.Id = Guid.NewGuid();

        var result = _pageModel.OnPost();

        result.Should().BeOfType<RedirectToPageResult>()
         .Which.PageName.Should().Be("OrganisationInternationalIdentification");
        ((RedirectToPageResult)result).RouteValues.Should().ContainKey("country").WhoseValue.Should().Be("USA");
    }
}
