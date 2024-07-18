using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementNoInputModel : FormElementModel
{
    public override FormAnswer? GetAnswer()
    {
        return null;
    }

    public override void SetAnswer(FormAnswer? answer)
    {
    }
}