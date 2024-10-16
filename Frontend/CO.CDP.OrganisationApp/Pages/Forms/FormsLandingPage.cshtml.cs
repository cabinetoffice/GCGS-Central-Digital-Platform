using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class FormsLandingPage(
    IFormsClient formsClient,
    IFormsEngine formsEngine,
    IUserInfoService userInfoService,
    ITempDataService tempDataService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        SectionQuestionsResponse form;
        try
        {
            form = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        tempDataService.Remove($"Form_{OrganisationId}_{FormId}_{SectionId}_Questions");
        tempDataService.Remove($"Form_{OrganisationId}_{FormId}_{SectionId}_Answers");

        if (form.Section.Type != FormSectionType.Declaration)
        {
            if (await userInfoService.IsViewer())
            {
                return RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
            }

            if (form.AnswerSets.Count != 0)
            {
                if (form.AnswerSets.Any(a => a.FurtherQuestionsExempted == true))
                {
                    return RedirectToPage("FormsCheckFurtherQuestionsExempted", new { OrganisationId, FormId, SectionId });
                }
                else
                {
                    return RedirectToPage("FormsAnswerSetSummary", new { OrganisationId, FormId, SectionId });
                }
            }
            else
            {
                if (form.Section.CheckFurtherQuestionsExempted)
                {
                    return RedirectToPage("FormsCheckFurtherQuestionsExempted", new { OrganisationId, FormId, SectionId });
                }
            }
        }

        var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
        if (currentQuestion == null)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = currentQuestion.Id });
    }
}