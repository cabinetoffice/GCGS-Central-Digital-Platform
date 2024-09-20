using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

public class CreateApiKeyModel(IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Enter the api key name")]
    public string? ApiKeyName { get; set; }

    public async Task<IActionResult> OnPost()
    {
        try
        {
            if (!ModelState.IsValid) return Page();

            var apiKey = Guid.NewGuid().ToString();

            var registerApiKey = new RegisterAuthenticationKey(
                                    key: apiKey,
                                    name: ApiKeyName,
                                    organisationId: Id,
                                    revoked: false);

            await organisationClient.CreateAuthenticationKey(Id, registerApiKey);

            return RedirectToPage("NewApiKeyDetails", new { Id, apiKey });
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }
}