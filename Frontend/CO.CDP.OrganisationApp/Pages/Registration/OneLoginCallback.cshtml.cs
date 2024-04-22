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
                    ?? throw new Exception("Unable to retrive user info");

        Tenant.WebApiClient.Tenant? tenant;

        try
        {
            tenant = await tenantClient.LookupTenantAsync(userInfo.UserId);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            try
            {
                tenant = await tenantClient.CreateTenantAsync(
                   new NewTenant(
                       new TenantContactInfo(userInfo.Email, userInfo.PhoneNumber),
                       userInfo.UserId));
            }
            catch (ApiException)
            {
                throw new Exception("Unable to create tenant");
            }
        }

        session.Set(Session.RegistrationDetailsKey,
        new RegistrationDetails
        {
            TenantId = tenant.Id,
            Email = tenant.ContactInfo.Email
        });

        return RedirectToPage("YourDetails");
    }
}