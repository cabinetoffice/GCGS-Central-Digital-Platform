using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.ApiKeyManagement;

[Authorize(Policy = OrgScopeRequirement.Editor)]
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
                                    organisationId: Id);

            await organisationClient.CreateAuthenticationKey(Id, registerApiKey);

            return RedirectToPage("NewApiKeyDetails", new { Id, apiKey });
        }        
        catch (ApiException<OrganisationWebApiClient.ProblemDetails> aex)
        {
            MapApiExceptions(aex);
            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
    }

    private void MapApiExceptions(ApiException<OrganisationWebApiClient.ProblemDetails> aex)
    {
        var code = ExtractErrorCode(aex);

        if (!string.IsNullOrEmpty(code))
        {
            ModelState.AddModelError(string.Empty, code switch
            {
                ErrorCodes.APIKEY_NAME_ALREADY_EXISTS => ErrorMessagesList.DuplicateApiKeyName,
                _ => ErrorMessagesList.UnexpectedError
            });
        }
    }

    private static string? ExtractErrorCode(ApiException<OrganisationWebApiClient.ProblemDetails> aex)
    {
        return aex.Result.AdditionalProperties.TryGetValue("code", out var code) && code is string codeString
            ? codeString
            : null;
    }
}