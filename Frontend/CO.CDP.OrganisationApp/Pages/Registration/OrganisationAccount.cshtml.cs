using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationAccountModel() : PageModel
{
}
