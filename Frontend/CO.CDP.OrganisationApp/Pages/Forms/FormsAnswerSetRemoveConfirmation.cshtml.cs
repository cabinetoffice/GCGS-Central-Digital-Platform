using CO.CDP.Forms.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormsAnswerSetRemoveConfirmationModel(
    IFormsClient formsClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid AnswerSetId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? ConfirmRemove { get; set; }

    public string? Caption { get; set; }

    public string? Heading { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var verify = await InitAndVerifyPage();
        if (!verify)
        {
            return Redirect("/page-not-found");
        }

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var verify = await InitAndVerifyPage();
        if (!verify)
        {
            return Redirect("/page-not-found");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (ConfirmRemove == true)
        {
            await formsClient.DeleteFormSectionAnswersAsync(AnswerSetId, OrganisationId);
        }

        return RedirectToPage("FormsAddAnotherAnswerSet", new { OrganisationId, FormId, SectionId });
    }

    private async Task<bool> InitAndVerifyPage()
    {
        try
        {
            var response = await formsClient.GetFormSectionQuestionsAsync(FormId, SectionId, OrganisationId);
            var valid = response.AnswerSets.FirstOrDefault(a => a.Id == AnswerSetId) != null;

            if (valid)
            {
                Caption = response.Section.Configuration.RemoveConfirmationCaption ?? response.Section.Title;
                Heading = response.Section.Configuration.RemoveConfirmationHeading ?? "Are you sure you want to remove this?";
            }

            return valid;
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return false;
        }
    }
}