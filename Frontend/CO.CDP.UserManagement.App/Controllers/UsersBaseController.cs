using CO.CDP.UserManagement.App.Attributes;
using CO.CDP.UserManagement.App.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.App.Controllers;

[Authorize(Policy = PolicyNames.OrganisationOwnerOrAdmin)]
[Route("organisation/{organisationSlug}")]
[OrganisationOwnerOrAdmin]
public abstract class UsersBaseController : Controller
{
}
