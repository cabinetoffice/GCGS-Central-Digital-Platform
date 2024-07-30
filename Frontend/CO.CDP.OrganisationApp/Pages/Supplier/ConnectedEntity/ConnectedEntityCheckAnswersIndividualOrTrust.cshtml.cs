using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityCheckAnswersIndividualOrTrustModel(
    ISession session,
    IOrganisationClient organisationClient) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    public string? Caption { get; set; }

    public string? Heading { get; set; }

    public ConnectedEntityState? ConnectedEntityDetails { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersIndividualOrTrust" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        try
        {
            await organisationClient.GetOrganisationAsync(Id);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        ConnectedEntityDetails = session.Get<ConnectedEntityState?>(Session.ConnectedPersonKey);

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModal(state);

        var payload = RegisterConnectedEntityPayload(state);

        if (payload == null)
        {
            ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullAurgument);
            return Page();
        }

        try
        {
            await organisationClient.RegisterConnectedPerson(Id, payload);

            session.Remove(Session.ConnectedPersonKey);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }

        return RedirectToPage("ConnectedPersonSummary", new { Id });
    }

    private void InitModal(ConnectedEntityState state)
    {
        Caption = state.GetCaption();
        Heading = $"Check your answers";
    }

    private RegisterConnectedEntity? RegisterConnectedEntityPayload(ConnectedEntityState state)
    {
        CreateConnectedOrganisation? connectedOrganisation = null;
        CreateConnectedIndividualTrust? connectedIndividualTrust = null;

        if (state.ConnectedEntityType == Constants.ConnectedEntityType.Individual ||
            state.ConnectedEntityType == Constants.ConnectedEntityType.TrustOrTrustee)
        {
            connectedIndividualTrust = new CreateConnectedIndividualTrust
            (
                category: state.ConnectedEntityIndividualAndTrustCategoryType!.Value.AsApiClientConnectedIndividualAndTrustCategory(),
                connectedType: (state.ConnectedEntityType == Constants.ConnectedEntityType.Individual
                                    ? ConnectedPersonType.Individual : ConnectedPersonType.TrustOrTrustee),
                controlCondition: state.ControlConditions.AsApiClientControlConditionList(),
                dateOfBirth: (state.DateOfBirth.HasValue ? state.DateOfBirth.Value : null),
                firstName: state.FirstName,
                lastName: state.LastName,
                nationality: state.Nationality,
                personId: null,
                residentCountry: state.DirectorLocation
            );
        }

        List<Address> addresses = new();

        if (state.RegisteredAddress != null)
        {
            addresses.Add(AddAddress(state.RegisteredAddress, Organisation.WebApiClient.AddressType.Registered));
        }

        if (state.PostalAddress != null)
        {
            addresses.Add(AddAddress(state.PostalAddress, Organisation.WebApiClient.AddressType.Postal));
        }

        var registerConnectedEntity = new RegisterConnectedEntity
        (
            addresses: addresses,
            companyHouseNumber: state.CompaniesHouseNumber,
            endDate: null,
            entityType: state.ConnectedEntityType!.Value.AsApiClientConnectedEntityType(),
            hasCompnayHouseNumber: (state.HasCompaniesHouseNumber ?? false),
            individualOrTrust: connectedIndividualTrust,
            organisation: connectedOrganisation,
            overseasCompanyNumber: state.OscCompaniesHouseNumber,
            registeredDate: (state.RegistrationDate.HasValue ? state.RegistrationDate.Value : null),
            registerName: state.RegisterName,
            startDate: null
        );

        return registerConnectedEntity;
    }

    private Address AddAddress(ConnectedEntityState.Address addressDetails, Organisation.WebApiClient.AddressType addressType)
    {
        return new Address(
            countryName: addressDetails.Country,
            locality: addressDetails.TownOrCity,
            postalCode: addressDetails.Postcode,
            region: null,
            streetAddress: addressDetails.AddressLine1,
            type: addressType);

    }

    private (bool valid, ConnectedEntityState state) ValidatePage()
    {
        var cp = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (cp == null ||
            cp.SupplierOrganisationId != Id ||
            (ConnectedEntityId.HasValue && cp.ConnectedEntityId.HasValue && cp.ConnectedEntityId != ConnectedEntityId))
        {
            return (false, new());
        }
        return (true, cp);
    }
}