using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class JoinOrganisationSuccessModel(ISession session) : PageModel
{
    public string? OrganisationName { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public IActionResult OnGet()
    {
        try
        {
            var jor = session.Get<JoinOrganisationRequestState>(Session.JoinOrganisationRequest);

            if (jor == null || jor.OrganisationId != Id)
            {
                return Redirect("/organisation-selection");
            }

            OrganisationName = jor.OrganisationName;

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
        finally
        {
            session.Remove(Session.JoinOrganisationRequest);
        }
    }
}