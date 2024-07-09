using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class DynamicFormsPageModel(IFormsEngine formsEngine) : PageModel
{
    public SectionQuestionsResponse? SectionWithQuestions { get; set; }

    public async Task OnGetAsync(Guid formId, Guid sectionId)
    {
        SectionWithQuestions = await formsEngine.LoadFormSectionAsync(formId, sectionId);
    }

    public IActionResult OnPost()
    {
        return RedirectToPage("/Forms/Confirmation");
    }
}
