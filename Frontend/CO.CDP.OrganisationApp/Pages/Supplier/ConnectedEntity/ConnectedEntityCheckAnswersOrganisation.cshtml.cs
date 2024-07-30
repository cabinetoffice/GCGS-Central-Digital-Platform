using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ConnectedEntityType = CO.CDP.OrganisationApp.Constants.ConnectedEntityType;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityCheckAnswersOrganisationModel(
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
                "ConnectedEntityCheckAnswersOrganisation" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
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

    public async Task<IActionResult> OnGetChange(Guid connectedEntityId)
    {
        try
        {
            await organisationClient.GetOrganisationAsync(Id);

            var connectedEntity = await organisationClient.GetConnectedEntityAsync(Id, connectedEntityId);

            ConnectedEntityDetails = GetConnectedEntityStateFromEntity(connectedEntity);

            InitModal(ConnectedEntityDetails);

            session.Set<ConnectedEntityState?>(Session.ConnectedPersonKey, ConnectedEntityDetails);

            return Page();
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            return Redirect("/page-not-found");
        }
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
            if (ConnectedEntityId.HasValue)
            {
                var updatePayload = UpdateConnectedEntityPayload(state);

                if (updatePayload == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullAurgument);
                    return Page();
                }

                await organisationClient.UpdateConnectedPerson(Id, ConnectedEntityId.Value, updatePayload);
            }
            else
            {
                await organisationClient.RegisterConnectedPerson(Id, payload);

                session.Remove(Session.ConnectedPersonKey);
            }
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

        if (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation)
        {
            connectedOrganisation = new CreateConnectedOrganisation
            (
                category: state.ConnectedEntityOrganisationCategoryType!.Value.AsApiClientConnectedEntityOrganisationCategoryType(),
                controlCondition: state.ControlConditions.AsApiClientControlConditionList(),
                insolvencyDate: state.InsolvencyDate,
                lawRegistered: state.LawRegistered,
                name: state.OrganisationName,
                organisationId: null,
                registeredLegalForm: state.LegalForm
            );
        }
        else
        {
            connectedIndividualTrust = new CreateConnectedIndividualTrust
            (
                category: state.ConnectedEntityIndividualAndTrustCategoryType!.Value.AsApiClientConnectedIndividualAndTrustCategory(),
                connectedType: (state.ConnectedEntityType == Constants.ConnectedEntityType.Individual
                                    ? ConnectedPersonType.Individual : ConnectedPersonType.TrustOrTrustee),
                controlCondition: state.ControlConditions.AsApiClientControlConditionList(),
                dateOfBirth: null,
                firstName: "",
                lastName: "",
                nationality: "",
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
            hasCompnayHouseNumber: state.SupplierHasCompanyHouseNumber!.Value,
            individualOrTrust: connectedIndividualTrust,
            organisation: connectedOrganisation,
            overseasCompanyNumber: "",
            registeredDate: (state.RegistrationDate.HasValue ? state.RegistrationDate.Value : null),
            registerName: state.RegisterName,
            startDate: null
        );

        return registerConnectedEntity;
    }

    private UpdateConnectedEntity? UpdateConnectedEntityPayload(ConnectedEntityState state)
    {
        UpdateConnectedOrganisation? connectedOrganisation = null;
        UpdateConnectedIndividualTrust? connectedIndividualTrust = null;

        if (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation)
        {
            connectedOrganisation = new UpdateConnectedOrganisation
            (
                category: state.ConnectedEntityOrganisationCategoryType!.Value.AsApiClientConnectedEntityOrganisationCategoryType(),
                controlCondition: state.ControlConditions.AsApiClientControlConditionList(),
                insolvencyDate: state.InsolvencyDate,
                lawRegistered: state.LawRegistered,
                name: state.OrganisationName,
                organisationId: null,
                registeredLegalForm: state.LegalForm
            );
        }
        else
        {
            connectedIndividualTrust = new UpdateConnectedIndividualTrust
            (
                category: state.ConnectedEntityIndividualAndTrustCategoryType!.Value.AsApiClientConnectedIndividualAndTrustCategory(),
                connectedType: (state.ConnectedEntityType == Constants.ConnectedEntityType.Individual
                                    ? ConnectedPersonType.Individual : ConnectedPersonType.TrustOrTrustee),
                controlCondition: state.ControlConditions.AsApiClientControlConditionList(),
                dateOfBirth: null,
                firstName: "",
                lastName: "",
                nationality: "",
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

        var updateConnectedEntity = new UpdateConnectedEntity
        (
            id: ConnectedEntityId.ToString(),
            addresses: addresses,
            companyHouseNumber: state.CompaniesHouseNumber,
            endDate: null,
            entityType: state.ConnectedEntityType!.Value.AsApiClientConnectedEntityType(),
            hasCompnayHouseNumber: state.HasCompaniesHouseNumber!.Value,
            individualOrTrust: connectedIndividualTrust,
            organisation: connectedOrganisation,
            overseasCompanyNumber: "",
            registeredDate: state.RegistrationDate!.Value,
            registerName: state.RegisterName,
            startDate: null
        );

        return updateConnectedEntity;
    }

    private ConnectedEntityState GetConnectedEntityStateFromEntity(CO.CDP.Organisation.WebApiClient.ConnectedEntity connectedEntity)
    {
        var connectedEntityType = connectedEntity.EntityType.AsConnectedEntityType();
        var controlConditions = connectedEntityType == ConnectedEntityType.Organisation
            ? connectedEntity.Organisation?.ControlCondition
            : connectedEntity.IndividualOrTrust?.ControlCondition;

        var state = new ConnectedEntityState()
        {
            CompaniesHouseNumber = connectedEntity.CompanyHouseNumber,
            ConnectedEntityId = connectedEntity.Id,
            ConnectedEntityIndividualAndTrustCategoryType = connectedEntity.IndividualOrTrust?.Category.AsConnectedEntityIndividualAndTrustCategoryType(),
            ConnectedEntityOrganisationCategoryType = connectedEntity.Organisation?.Category.AsConnectedEntityOrganisationCategoryType(),
            ConnectedEntityType = connectedEntityType,
            HasCompaniesHouseNumber = connectedEntity.HasCompnayHouseNumber,
            InsolvencyDate = connectedEntity.Organisation?.InsolvencyDate,
            LawRegistered = connectedEntity.Organisation?.LawRegistered,
            LegalForm = connectedEntity.Organisation?.RegisteredLegalForm,
            OrganisationName = connectedEntity.Organisation?.Name,
            PostalAddress = new ConnectedEntityState.Address
            {
                AddressLine1 = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Postal)?.StreetAddress,
                Country = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Postal)?.CountryName,
                Postcode = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Postal)?.PostalCode,
                TownOrCity = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Postal)?.Locality
            },
            RegistrationDate = connectedEntity.RegisteredDate,
            RegisterName = connectedEntity.RegisterName,
            RegisteredAddress = new ConnectedEntityState.Address
            {
                AddressLine1 = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Registered)?.StreetAddress,
                Country = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Registered)?.CountryName,
                Postcode = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Registered)?.PostalCode,
                TownOrCity = connectedEntity.Addresses?.FirstOrDefault(a => a.Type == Organisation.WebApiClient.AddressType.Registered)?.Locality
            },
            ControlConditions = ControlConditionCollectionToList(controlConditions),
            Nationality = connectedEntity.IndividualOrTrust?.Nationality,
            SupplierOrganisationId = Id,
            FirstName = connectedEntity.IndividualOrTrust?.FirstName,
            LastName = connectedEntity.IndividualOrTrust?.LastName,
            DateOfBirth = connectedEntity.IndividualOrTrust?.DateOfBirth,
            OscCompaniesHouseNumber = connectedEntity.OverseasCompanyNumber,
            HasOscCompaniesHouseNumber = connectedEntity.OverseasCompanyNumber != null,
            SupplierHasCompanyHouseNumber = connectedEntity.HasCompnayHouseNumber,
        };

        return state;
    }

    private List<ConnectedEntityControlCondition> ControlConditionCollectionToList(ICollection<ControlCondition>? controlConditionCollection)
    {
        List<ConnectedEntityControlCondition> controlConditions = new();

        if (controlConditionCollection == null)
        {
            return controlConditions;
        }

        foreach (var cc in controlConditionCollection)
        {
            var controlConditionCe = cc.AsConnectedEntityClientControlCondition();
            controlConditions.Add(controlConditionCe);
        }

        return controlConditions;
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