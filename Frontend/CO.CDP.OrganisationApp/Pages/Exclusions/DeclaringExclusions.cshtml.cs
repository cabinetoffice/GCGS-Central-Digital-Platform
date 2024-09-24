using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Exclusions;

public class DeclaringExclusionsModel(IFormsEngine formsEngine) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid OrganisationId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid FormId { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid SectionId { get; set; }

    [BindProperty]
    public bool? YesNoInput { get; set; }

    public IActionResult OnPost()
    {
        if (!YesNoInput.HasValue)
            ModelState.AddModelError(nameof(YesNoInput), "Please select an option");

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (YesNoInput == true)
        {
            return RedirectToPage("../Forms/DynamicFormsPage", new { OrganisationId, FormId, SectionId });
        }

        formsEngine.SaveUpdateAnswers(FormId, SectionId, OrganisationId, new FormQuestionAnswerState() { FurtherQuestionsExempted = true });

        return RedirectToPage("../Supplier/SupplierInformationSummary", new { Id = OrganisationId });
    }
}
