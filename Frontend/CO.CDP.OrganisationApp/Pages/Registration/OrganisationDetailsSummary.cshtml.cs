using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Models;
using CO.CDP.Person.WebApiClient;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrganisationWebApiClient = CO.CDP.Organisation.WebApiClient;
using PersonWebApiClient = CO.CDP.Person.WebApiClient;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[Authorize]
public class OrganisationDetailsSummaryModel(
    ISession session,
    IOrganisationClient organisationClient,
    IPersonClient personClient, ITenantClient tenantClient) : PageModel
{
    public RegistrationDetails? Details { get; set; }

    [BindProperty]
    public string? Error { get; set; }

    public void OnGet()
    {
        Details = VerifySession();
    }

    public async Task<IActionResult> OnPost()
    {
        Details = VerifySession();

        var organisation = await RegisterOrganisation();
        if (organisation != null)
        {
            Details.OrganisationId = organisation.Id;
            session.Set(Session.RegistrationDetailsKey, Details);
        }
        else
        {
            return Page();
        }

        var person = await RegisterPerson();
        if (person != null)
        {
            Details.PersonId = person.Id;
            session.Set(Session.RegistrationDetailsKey, Details);
        }
        else
        {
            return Page();
        }

        await tenantClient.AssignUserToOrganisationAsync(
            Details.TenantId.ToString(), new AssignUserToOrganisation(organisation.Id, person.Id));

        return RedirectToPage("OrganisationAccount");
    }

    private async Task<OrganisationWebApiClient.Organisation?> RegisterOrganisation()
    {
        OrganisationWebApiClient.Organisation? organisation = null;

        try
        {
            if (Details!.OrganisationId.HasValue)
            {
                organisation = await organisationClient.GetOrganisationAsync(Details.OrganisationId.Value);
            }
            else
            {
                try
                {
                    organisation = await organisationClient.CreateOrganisationAsync
                        (new NewOrganisation(
                            null,
                            new OrganisationAddress(
                                Details.OrganisationAddressLine1,
                                Details.OrganisationAddressLine2,
                                Details.OrganisationCityOrTown,
                                Details.OrganisationCountry,
                                Details.OrganisationPostcode),
                            new OrganisationContactPoint(
                                Details.OrganisationEmailAddress,
                                null,
                                null,
                                null),
                            new OrganisationIdentifier(
                                null,
                                null,
                                Details.OrganisationIdentificationNumber,
                                Details.OrganisationScheme,
                                null),
                            Details.OrganisationName,
                            [1] // TODO: Need to update - Hard-coded till we have buyer/supplier screen
                        ));
                }
                catch (OrganisationWebApiClient.ApiException aex)
                    when (aex is OrganisationWebApiClient.ApiException<OrganisationWebApiClient.ProblemDetails> pd)
                {
                    Error = pd.Result.Detail;
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return organisation;
    }

    private async Task<PersonWebApiClient.Person?> RegisterPerson()
    {
        PersonWebApiClient.Person? person = null;

        try
        {
            if (Details!.PersonId.HasValue)
            {
                person = await personClient.GetPersonAsync(Details.PersonId.Value);
            }
            else
            {
                try
                {
                    person = await personClient.LookupPersonAsync(Details.Email);
                }
                catch (PersonWebApiClient.ApiException ex) when (ex.StatusCode == 404)
                {
                    try
                    {
                        person = await personClient.CreatePersonAsync(new NewPerson(
                            email: Details.Email,
                            phone: Details.Phone,
                            firstName: Details.FirstName,
                            lastName: Details.LastName,
                            userUrn: Details.UserPrincipal
                        ));
                    }
                    catch (PersonWebApiClient.ApiException aex)
                        when (aex is PersonWebApiClient.ApiException<PersonWebApiClient.ProblemDetails> pd)
                    {
                        Error = pd.Result.Detail;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Error = ex.Message;
        }

        return person;
    }

    private RegistrationDetails VerifySession()
    {
        var registrationDetails = session.Get<RegistrationDetails>(Session.RegistrationDetailsKey);
        if (registrationDetails == null)
        {
            //show error page (Once we finalise)
            throw new Exception("Shoudn't be here");
        }
        return registrationDetails;
    }
}