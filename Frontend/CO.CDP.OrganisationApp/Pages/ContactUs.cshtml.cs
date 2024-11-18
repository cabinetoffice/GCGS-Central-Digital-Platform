using CO.CDP.Localization;
using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Validation;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages;

[AuthenticatedSessionNotRequired]
public class ContactUsModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true, Name = "message-sent")]
    public bool? MessageSent { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Supplementary_ContactUs_Name))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplementary_ContactUs_Name_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? Name { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Supplementary_ContactUs_Email))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplementary_ContactUs_Email_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [ValidEmailAddress(ErrorMessageResourceName = nameof(StaticTextResource.Global_Email_Invalid_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? EmailAddress { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Supplementary_ContactUs_Organisation))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplementary_ContactUs_Organisation_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    public string? OrganisationName { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Supplementary_ContactUs_Message))]
    [Required(ErrorMessageResourceName = nameof(StaticTextResource.Supplementary_ContactUs_Message_ErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [MinLength(10, ErrorMessageResourceName = nameof(StaticTextResource.Supplementary_ContactUs_Message_ValidationErrorMessage), ErrorMessageResourceType = typeof(StaticTextResource))]
    [MaxLength(10000)]
    public string? Message { get; set; }

    public string? Error { get; set; }

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var contactus = new ContactUs(EmailAddress, Message, Name, OrganisationName);

        await organisationClient.ContactUs(contactus);

        return Redirect("/contact-us?message-sent=true");

    }
}