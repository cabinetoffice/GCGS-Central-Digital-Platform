using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Exclusions;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Exclusions;
public class DeclaringExclusionsTests
{
    private readonly Mock<IFormsEngine> _formsEngineMock;

    public DeclaringExclusionsTests()
    {
        _formsEngineMock = new Mock<IFormsEngine>();
    }
    [Fact]
    public void OnPost_ShouldReturnPage_WhenNoOptionIsSelected()
    {
        var pageModel = new DeclaringExclusionsModel(_formsEngineMock.Object)
        {
            OrganisationId = Guid.NewGuid(),
            FormId = Guid.NewGuid(),
            SectionId = Guid.NewGuid(),
            YesNoInput = null
        };

        var result = pageModel.OnPost();

        result.Should().BeOfType<PageResult>();
        pageModel.ModelState.IsValid.Should().BeFalse();
        pageModel.ModelState[nameof(DeclaringExclusionsModel.YesNoInput)]?.Errors.Should().Contain(e => e.ErrorMessage == "Please select an option");
    }

    [Fact]
    public void OnPost_ShouldRedirectToCorrectPage_WhenYesOptionIsSelected()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();

        var pageModel = new DeclaringExclusionsModel(_formsEngineMock.Object)
        {
            OrganisationId = organisationId,
            FormId = formId,
            SectionId = sectionId,
            YesNoInput = true
        };

        var result = pageModel.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        _formsEngineMock.Verify(engine => engine.SaveUpdateAnswers(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<FormQuestionAnswerState>()
        ), Times.Never);
        redirectResult!.PageName.Should().Be("");
        redirectResult.RouteValues.Should().ContainKey("OrganisationId").WhoseValue.Should().Be(organisationId);
        redirectResult.RouteValues.Should().ContainKey("FormId").WhoseValue.Should().Be(formId);
        redirectResult.RouteValues.Should().ContainKey("SectionId").WhoseValue.Should().Be(sectionId);
    }

    [Fact]
    public void OnPost_ShouldRedirectToSummaryPage_WhenNoOptionIsSelected()
    {
        var organisationId = Guid.NewGuid();

        var pageModel = new DeclaringExclusionsModel(_formsEngineMock.Object)
        {
            OrganisationId = organisationId,
            YesNoInput = false
        };

        var result = pageModel.OnPost();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;
        _formsEngineMock.Verify(engine => engine.SaveUpdateAnswers(
          It.IsAny<Guid>(),
          It.IsAny<Guid>(),
          organisationId,
          It.Is<FormQuestionAnswerState>(s => s.FurtherQuestionsExempted == true)
        ), Times.Once);
        redirectResult!.PageName.Should().Be("../Supplier/SupplierInformationSummary");
        redirectResult.RouteValues.Should().ContainKey("Id").WhoseValue.Should().Be(organisationId);

    }

}
