using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormsCheckFurtherQuestionsExemptedTest
{
    private readonly Mock<IFormsEngine> _formsEngineMock = new();
    private readonly Mock<IFormsClient> _formsClientMock = new();

    [Fact]
    public async Task OnPost_ShouldRedirectToCorrectPage_WhenYesOptionIsSelected()
    {
        var organisationId = Guid.NewGuid();
        var formId = Guid.NewGuid();
        var sectionId = Guid.NewGuid();
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(organisationId, formId, sectionId, null))
            .ReturnsAsync(new Models.FormQuestion());

        var pageModel = new FormsCheckFurtherQuestionsExemptedModel(_formsClientMock.Object, _formsEngineMock.Object)
        {
            OrganisationId = organisationId,
            FormId = formId,
            SectionId = sectionId,
            Confirm = true
        };

        var result = await pageModel.OnPostAsync();

        result.Should().BeOfType<RedirectToPageResult>();
        var redirectResult = result as RedirectToPageResult;

        redirectResult!.PageName.Should().Be("FormsQuestionPage");
        redirectResult.RouteValues.Should().ContainKey("OrganisationId").WhoseValue.Should().Be(organisationId);
        redirectResult.RouteValues.Should().ContainKey("FormId").WhoseValue.Should().Be(formId);
        redirectResult.RouteValues.Should().ContainKey("SectionId").WhoseValue.Should().Be(sectionId);
    }

    [Fact]
    public async Task OnPost_ShouldRedirectToSummaryPage_WhenNoOptionIsSelected()
    {
        var organisationId = Guid.NewGuid();
        _formsEngineMock.Setup(f => f.GetCurrentQuestion(organisationId, It.IsAny<Guid>(), It.IsAny<Guid>(), null))
            .ReturnsAsync(new Models.FormQuestion());

        var pageModel = new FormsCheckFurtherQuestionsExemptedModel(_formsClientMock.Object, _formsEngineMock.Object)
        {
            OrganisationId = organisationId,
            Confirm = false
        };

        var result = await pageModel.OnPostAsync();

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