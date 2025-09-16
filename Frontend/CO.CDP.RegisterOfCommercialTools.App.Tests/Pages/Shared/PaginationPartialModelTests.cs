using CO.CDP.RegisterOfCommercialTools.App.Pages.Shared;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.RegisterOfCommercialTools.App.Tests.Pages.Shared;

public class PaginationPartialModelTests
{
    [Fact]
    public void TotalPages_CalculatesCorrectly_WhenDivisible()
    {
        var model = new PaginationPartialModel
        {
            TotalItems = 100,
            PageSize = 10,
            CurrentPage = 1,
            Url = "/test"
        };

        var totalPages = model.TotalPages;

        totalPages.Should().Be(10);
    }

    [Fact]
    public void TotalPages_CalculatesCorrectly_WhenNotDivisible()
    {
        var model = new PaginationPartialModel
        {
            TotalItems = 101,
            PageSize = 10,
            CurrentPage = 1,
            Url = "/test"
        };

        var totalPages = model.TotalPages;

        totalPages.Should().Be(11);
    }

    [Fact]
    public void GetPageUrl_ReturnsCorrectUrl()
    {
        var model = new PaginationPartialModel
        {
            TotalItems = 100,
            PageSize = 10,
            CurrentPage = 1,
            Url = "/test"
        };

        var pageUrl = model.GetPageUrl(2);

        pageUrl.Should().Be("/test?pageNumber=2");
    }

    [Fact]
    public void GetPageUrlWhenUrlAlreadyHaveQuerystring_ReturnsCorrectUrl()
    {
        var model = new PaginationPartialModel
        {
            TotalItems = 100,
            PageSize = 10,
            CurrentPage = 1,
            Url = "/test?q=hello"
        };

        var pageUrl = model.GetPageUrl(2);

        pageUrl.Should().Be("/test?q=hello&pageNumber=2");
    }

    [Fact]
    public void Model_WithAllRequiredProperties_ShouldPassValidation()
    {
        var model = new PaginationPartialModel
        {
            CurrentPage = 1,
            TotalItems = 100,
            PageSize = 10,
            Url = "/test"
        };

        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(model);

        var isValid = Validator.TryValidateObject(model, validationContext, validationResults, true);

        isValid.Should().BeTrue();
        validationResults.Should().BeEmpty();
    }
}