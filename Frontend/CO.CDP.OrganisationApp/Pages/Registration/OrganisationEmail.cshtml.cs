using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.ThirdPartyApiClients.CharityCommission;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[ValidateRegistrationStep]
public class OrganisationEmailModel(ISession session, ICharityCommissionApi charityCommissionApi) : RegistrationStepModel(session)
{
    public override string CurrentPage => OrganisationEmailPage;

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Organisation_Email_Heading))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Organisation_Email_Required_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EmailAddress { get; set; }

    [BindProperty]
    public bool? RedirectToSummary { get; set; }

    public async Task OnGet()
    {
        EmailAddress = RegistrationDetails.OrganisationEmailAddress;

        if ((RegistrationDetails.OrganisationScheme == OrganisationSchemeType.CharityCommissionEnglandWales) && (string.IsNullOrEmpty(EmailAddress)))
        {
            if (RegistrationDetails.OrganisationIdentificationNumber != null)
            {
                var details = await charityCommissionApi.GetCharityDetails(RegistrationDetails.OrganisationIdentificationNumber);

                if (details != null)
                {
                    EmailAddress = details.Email;
                }
            }
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        RegistrationDetails.OrganisationEmailAddress = EmailAddress;

        SessionContext.Set(Session.RegistrationDetailsKey, RegistrationDetails);

        if (RedirectToSummary == true)
        {
            return RedirectToPage("OrganisationDetailsSummary");
        }
        else
        {
            return RedirectToPage("OrganisationRegisteredAddress", new { UkOrNonUk = "uk" });
        }
    }
}
