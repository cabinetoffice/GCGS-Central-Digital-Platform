using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.BuyerParentChildRelationship;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.BuyerParentChildRelationship;

public class ChildOrganisationSearchPageTests
{
    private readonly ChildOrganisationSearchPage _page;

    public ChildOrganisationSearchPageTests()
    {
        var mockOrganisationClient = new Mock<IOrganisationClient>();
        _page = new ChildOrganisationSearchPage(mockOrganisationClient.Object);
    }

    [Fact]
    public void OnGet_ReturnsPageResult()
    {
        _page.Id = Guid.NewGuid();

        var result = _page.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_EmptyQuery_ReturnsBadRequest()
    {
        _page.Query = string.Empty;

        var validationContext = new ValidationContext(_page);
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateObject(_page, validationContext, validationResults, true);

        foreach (var validationResult in validationResults)
        {
            foreach (var memberName in validationResult.MemberNames)
            {
                if (validationResult.ErrorMessage != null)
                    _page.ModelState.AddModelError(memberName, validationResult.ErrorMessage);
            }
        }

        var result = _page.OnPost();

        result.Should().BeOfType<PageResult>();
        _page.ModelState.IsValid.Should().BeFalse();
        _page.ModelState.Should().ContainKey("Query");
    }
}
