using CO.CDP.Organisation.WebApiClient;
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
    [DisplayName("Name")]
    [Required(ErrorMessage = "Enter your name")]
    public string? Name { get; set; }

    [BindProperty]
    [DisplayName("Email")]
    [Required(ErrorMessage = "Enter your email")]
    [EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
    public string? EmailAddress { get; set; }

    [BindProperty]
    [DisplayName("Organisation")]
    [Required(ErrorMessage = "Enter your Organisation")]
    public string? OrganisationName { get; set; }

    [BindProperty]
    [DisplayName("Message")]
    [Required(ErrorMessage = "Enter your message")]
    [MinLength(10, ErrorMessage = "Message must be between 10 and 10,000 characters")]
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