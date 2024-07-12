using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Pages.Supplier;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Supplier.ConnectedEntity;

public class ConnectedPersonDeclarationTest
{
    private readonly ConnectedPersonDeclarationModel _model;

    public ConnectedPersonDeclarationTest()
    {
        _model = new ConnectedPersonDeclarationModel();
    }

    [Fact]
    public void OnGet_ShouldReturnPageResult()
    {
        var model = new ConnectedPersonDeclarationModel();

        var result = model.OnGet();

        result.Should().BeOfType<PageResult>();
    }

    [Fact]
    public void OnPost_ShouldRedirectToConnectedQuestionPageWithId()
    {
        var model = new ConnectedPersonDeclarationModel { Id = Guid.NewGuid() };

        var result = model.OnPost();

        var redirectToPageResult = result.Should().BeOfType<RedirectToPageResult>().Subject;

        result.Should().BeOfType<RedirectToPageResult>()
            .Which.PageName.Should().Be("ConnectedQuestion");

    }
}