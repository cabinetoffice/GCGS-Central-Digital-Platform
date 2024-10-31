using CO.CDP.Forms.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class FormsCheckFurtherQuestionsExemptedModel(IFormsClient formsClient, IFormsEngine formsEngine) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }


    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? Confirm { get; set; }

    public string? Heading { get; set; }
    public string? Hint { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await InitModel();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await InitModel();
            return Page();
        }

        if (Confirm == true)
        {
            var currentQuestion = await formsEngine.GetCurrentQuestion(OrganisationId, FormId, SectionId, null);
            if (currentQuestion == null)
            {
                return Redirect("/page-not-found");
            }

            return RedirectToPage("FormsQuestionPage", new { OrganisationId, FormId, SectionId, CurrentQuestionId = currentQuestion.Id });
        }

        await formsEngine.SaveUpdateAnswers(FormId, SectionId, OrganisationId, new FormQuestionAnswerState { FurtherQuestionsExempted = true });

        return RedirectToPage("../Supplier/SupplierInformationSummary", new { Id = OrganisationId });
    }

    private async Task InitModel()
    {
        var form = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
        Heading = form?.Section?.Configuration?.FurtherQuestionsExemptedHeading;
        Hint = form?.Section?.Configuration?.FurtherQuestionsExemptedHint;
    }
}