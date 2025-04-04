using CO.CDP.Organisation.WebApiClient;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;
using CO.CDP.OrganisationApp.WebApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize(Policy = OrgScopeRequirement.Editor)]
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
    public string? BackPageLink { get; set; }
    public bool ShowRegisterDate { get; set; }
    public bool ShowLegalForm { get; set; }
    public bool ShowCompaniesHouse { get; set; }
    public bool ShowOverseasCompanyHouse { get; set; }

    public async Task<IActionResult> OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswersOrganisation" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModel(state);

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

            ConnectedEntityDetails = ConnectedEntityCheckAnswersCommon.GetConnectedEntityStateFromEntity(Id, connectedEntity);

            InitModel(ConnectedEntityDetails);

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

        InitModel(state);

        try
        {
            if (ConnectedEntityId.HasValue)
            {
                var updatePayload = UpdateConnectedEntityPayload(state);

                if (updatePayload == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullArgument);
                    return Page();
                }

                await organisationClient.UpdateConnectedPerson(Id, ConnectedEntityId.Value, updatePayload);
            }
            else
            {
                var payload = RegisterConnectedEntityPayload(state);

                if (payload == null)
                {
                    ModelState.AddModelError(string.Empty, ErrorMessagesList.PayLoadIssueOrNullArgument);
                    return Page();
                }

                await organisationClient.RegisterConnectedPerson(Id, payload);
            }

            session.Remove(Session.ConnectedPersonKey);
        }
        catch (ApiException ex) when (ex.StatusCode == 404)
        {
            session.Remove(Session.ConnectedPersonKey);

            return Redirect("/page-not-found");
        }

        return RedirectToPage("ConnectedPersonSummary", new { Id });
    }

    private void InitModel(ConnectedEntityState state)
    {
        Caption = state.GetCaption();
        Heading = StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityCheckAnswersOrganisation_PageTitle;
        BackPageLink = GetBackLinkPageName(state);
        ShowRegisterDate = ConnectedEntityCheckAnswersCommon.SetShowRegisterDate(state);
        ShowLegalForm = ConnectedEntityCheckAnswersCommon.SetShowLegalForm(state);
        ShowCompaniesHouse = ConnectedEntityCheckAnswersCommon.SetShowCompaniesHouse(state);
        ShowOverseasCompanyHouse = ConnectedEntityCheckAnswersCommon.SetShowOverseasCompanyHouse(state);
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

        List<Address> addresses = new();

        if (state.RegisteredAddress?.AddressLine1 != null)
        {
            addresses.Add(AddAddress(state.RegisteredAddress, CO.CDP.Organisation.WebApiClient.AddressType.Registered));
        }

        if (state.PostalAddress?.AddressLine1 != null)
        {
            addresses.Add(AddAddress(state.PostalAddress, CO.CDP.Organisation.WebApiClient.AddressType.Postal));
        }

        var registerConnectedEntity = new RegisterConnectedEntity
        (
            addresses: addresses,
            companyHouseNumber: (state.HasCompaniesHouseNumber == null ? null : (state.CompaniesHouseNumber ?? "")),
            endDate: null,
            entityType: state.ConnectedEntityType!.Value.AsApiClientConnectedEntityType(),
            hasCompnayHouseNumber: state.SupplierHasCompanyHouseNumber!.Value,
            individualOrTrust: connectedIndividualTrust,
            organisation: connectedOrganisation,
            overseasCompanyNumber: state.OverseasCompaniesHouseNumber,
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

        List<Address> addresses = new();

        if (state.RegisteredAddress != null)
        {
            addresses.Add(AddAddress(state.RegisteredAddress, CO.CDP.Organisation.WebApiClient.AddressType.Registered));
        }

        if (state.PostalAddress != null)
        {
            addresses.Add(AddAddress(state.PostalAddress, CO.CDP.Organisation.WebApiClient.AddressType.Postal));
        }

        var updateConnectedEntity = new UpdateConnectedEntity
        (
            id: ConnectedEntityId.ToString(),
            addresses: addresses,
            companyHouseNumber: state.CompaniesHouseNumber,
            endDate: state.EndDate,
            entityType: state.ConnectedEntityType!.Value.AsApiClientConnectedEntityType(),
            hasCompnayHouseNumber: state.SupplierHasCompanyHouseNumber!.Value,
            individualOrTrust: connectedIndividualTrust,
            organisation: connectedOrganisation,
            overseasCompanyNumber: state.OverseasCompaniesHouseNumber,
            registeredDate: (state.RegistrationDate.HasValue ? state.RegistrationDate.Value : null),
            registerName: state.RegisterName,
            startDate: null
        );

        return updateConnectedEntity;
    }

    private Address AddAddress(ConnectedEntityState.Address addressDetails, CO.CDP.Organisation.WebApiClient.AddressType addressType)
    {
        return new Address(
            countryName: addressDetails.CountryName,
            country: addressDetails.Country,
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
    private string GetBackLinkPageName(ConnectedEntityState state)
    {
        var backPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "company-register-name"
                                    : (state.RegistrationDate.HasValue == true
                                            ? "company-register-name"
                                            : "date-registered-question");
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "company-question"
                                    : (state.HasCompaniesHouseNumber == true
                                        ? "company-question"
                                        : "overseas-company-question");
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        backPage = "date-insolvency";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        backPage = state.SupplierHasCompanyHouseNumber == true
                                    ? "law-register"
                                    : (state.HasLegalForm == true ? "law-enforces" : "legal-form-question");
                        break;

                }
                break;
        }

        return backPage;
    }
}