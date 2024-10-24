using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using FluentAssertions;
using Xunit;
using CO.CDP.OrganisationApp.Pages.Registration;

namespace CO.CDP.OrganisationApp.Tests.Pages.Registration;

public class OrganisationAlreadyRegisteredTests
{
    [Fact]
    public void OnGet_ValidIdentifier_SetsIdentifierAndReturnsPageResult()
    {
        var pageModel = new OrganisationAlreadyRegistered();
        var testIdentifier = "GB-NHS:123456789";

        var result = pageModel.OnGet(testIdentifier);

        result.Should().BeOfType<PageResult>();
        pageModel.Identifier.Should().Be(testIdentifier);
    }

    [Fact]
    public void OnGet_NullIdentifier_SetsIdentifierToNullAndReturnsPageResult()
    {
        var pageModel = new OrganisationAlreadyRegistered();

        var result = pageModel.OnGet(null!);

        result.Should().BeOfType<PageResult>();
        pageModel.Identifier.Should().BeNull();
    }
}