using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Organisation.Supplier;

[Authorize]
public class SupplierTypeModel(ISession session) : PageModel
{
    [BindProperty]
    [Required(ErrorMessage = "Select the journey you want to take")]
    public SupplierType? SupplierType { get; set; }

    public IActionResult OnGet(Guid? id)
    {
        ArgumentNullException.ThrowIfNull(id);
        OrganisationDetails organisationDetails = new();
        if (session.Get<OrganisationDetails>(Session.OrganisationDetailsKey) == null)
        {
            session.Set(Session.OrganisationDetailsKey, new OrganisationDetails() { OrganisationId = id });
        }
        else
        {
            organisationDetails = VerifySession();

            if (organisationDetails.OrganisationId != id)
            {
                session.Set(Session.OrganisationDetailsKey, new OrganisationDetails() { OrganisationId = id });
            }
        }

        organisationDetails = VerifySession();

        SupplierType = organisationDetails.SupplierType;

        return Page();
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var organisationDetails = VerifySession();
        organisationDetails.SupplierType = SupplierType ?? organisationDetails.SupplierType;
        session.Set(Session.OrganisationDetailsKey, organisationDetails);

        return RedirectToPage("/Organisation/BasicInformation");

    }

    private OrganisationDetails VerifySession()
    {
        var organisationDetails = session.Get<OrganisationDetails>(Session.OrganisationDetailsKey)
            ?? throw new Exception(ErrorMessagesList.SessionNotFound);

        return organisationDetails;
    }
}