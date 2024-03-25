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
        // Retrieve user information
        var userInfo = await oneLoginClient.GetUserInfo()
                    ?? throw new Exception("Unable to retrive user info"); // show error page?

        // Do Tenant lookup
        //var tenantLookup = await tenantClient.LookupTenant(userInfo.UserId);
        dynamic tenantLookup = null;
        Tenant.WebApiClient.Tenant? tenant;

        if (tenantLookup == null)
        {
            // Create Tenant
            try
            {
                tenant = await tenantClient.RegisterTenant(new NewTenant
                {
                    Name = userInfo.UserId,
                    ContactInfo = new TenantContactInfo
                    {
                        Email = userInfo.Email,
                        Phone = userInfo.PhoneNumber
                    }
                });
            }
            catch (TenantClientException)
            {
                // show error page?
                throw new Exception("Unable to create tenant"); 
            }
        }
        else
        {
            // Retrieve Tenant
            tenant = await tenantClient.GetTenant(tenantLookup.UserId)
                        ?? throw new Exception("Get tenant error"); // show error page?
        }

        session.Set(Session.RegistrationDetailsKey,
            new RegistrationDetails
            {
                TenantId = tenant.Id,
                FirstName = tenant.Name,
                Email = tenant.ContactInfo.Email,
            });

        return RedirectToPage("YourDetails");
    }
}