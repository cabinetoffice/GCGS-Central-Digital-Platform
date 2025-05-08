using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Viewer)]
public class SupplierInformationBeforeYouStartModel : PageModel
{
}