using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class YourDetailsModel : PageModel
{
	[BindProperty]
	[DisplayName("First name")]
	[Required(ErrorMessage = "Enter your first name")]
	public string? FirstName { get; set; }

	[BindProperty]
	[DisplayName("Last name")]
	[Required(ErrorMessage = "Enter your last name")]
	public string? LastName { get; set; }

	[BindProperty]
	[DisplayName("Email address")]
	[Required(ErrorMessage = "Enter your email address")]
	[EmailAddress(ErrorMessage = "Enter an email address in the correct format, like name@example.com")]
	public string? Email { get; set; }

	public void OnGet()
	{
	}

	public IActionResult OnPost()
	{
		if (!ModelState.IsValid)
		{
			return Page();
		}

		return Page();
	}
}
