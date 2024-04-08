using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.ServiceClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Registration;

public class OneLoginCallback(
    IOneLoginClient oneLoginClient,
    ITenantClient tenantClient,
    ISession session) : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        var userInfo = await oneLoginClient.GetUserInfo()
                    ?? throw new Exception("Unable to retrive user info"); // show error page?

        try
        {
            var tenant = await tenantClient.CreateTenantAsync(
                new NewTenant(
                    new TenantContactInfo(userInfo.Email, userInfo.PhoneNumber),
                    userInfo.UserId));

            session.Set(Session.RegistrationDetailsKey,
            new RegistrationDetails
            {
                TenantId = tenant.Id,
                FirstName = tenant.Name,
                Email = tenant.ContactInfo.Email,
            });

            return RedirectToPage("YourDetails");
        }
        catch (ApiException)
        {
            // show error page?
            throw new Exception("Unable to create tenant");
        }

    }
}