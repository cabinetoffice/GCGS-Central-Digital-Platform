using CO.CDP.Organisation.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class SupplierQualificationAwardedBodyModel(
    ISession session,
    IOrganisationClient organisationClient) : LoggedInUserAwareModel
{
    public override ISession SessionContext => session;

    [BindProperty]
    [Required(ErrorMessage = "Please enter person or awarded body.")]
    public string? PersonOrAwardedBody { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public void OnGet()
    {
    }
}
