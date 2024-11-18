using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Supplier.ConnectedEntity;

[Authorize(Policy = OrgScopeRequirement.Editor)]
public class ConnectedEntityPostalSameAsRegisteredAddressModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = nameof(StaticTextResource.Supplier_ConnectedEntity_ConnectedEntityPostalSameAsRegisteredAddress_HasDifferentPostalAddressError))]
    public bool? DifferentThanRegiseterdAddress { get; set; }

    public string? Caption { get; set; }

    public string? OrganisationName { get; set; }

    public bool? IsNonUkAddress { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue
                ? (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
                    ? "ConnectedEntityCheckAnswersOrganisation"
                    : "ConnectedEntityCheckAnswersIndividualOrTrust")
                : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        var sameAddress = state.RegisteredAddress?.AreSameAddress(state.PostalAddress);

        DifferentThanRegiseterdAddress = selected.HasValue ? selected : (sameAddress.HasValue ? !sameAddress : sameAddress);

        InitModal(state);

        return Page();
    }

    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue
                ? (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
                    ? "ConnectedEntityCheckAnswersOrganisation"
                    : "ConnectedEntityCheckAnswersIndividualOrTrust")
                : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);
        if (!ModelState.IsValid)
        {
            return Page();
        }
        state.PostalAddress = null;
        if (DifferentThanRegiseterdAddress == false)
        {
            state.PostalAddress = state.RegisteredAddress;
            session.Set(Session.ConnectedPersonKey, state);

            var redirectLink = GetRedirectLinkPageName(state);
            return RedirectToPage(redirectLink, new { Id, ConnectedEntityId });
        }


        return RedirectToPage("ConnectedEntityAddress",
            new { Id, ConnectedEntityId, AddressType = AddressType.Postal, UkOrNonUk = "uk" });
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

    private void InitModal(ConnectedEntityState state)
    {
        OrganisationName = state.OrganisationName;
        Caption = state.GetCaption();
        IsNonUkAddress = state.RegisteredAddress?.IsNonUk;
    }

    private string GetRedirectLinkPageName(ConnectedEntityState state)
    {
        var redirectPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                switch (state.ConnectedEntityOrganisationCategoryType)
                {
                    case ConnectedEntityOrganisationCategoryType.RegisteredCompany:
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        redirectPage = state.SupplierHasCompanyHouseNumber == true
                                        ? "ConnectedEntityLawRegister"
                                        : "ConnectedEntityLegalFormQuestion";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = "ConnectedEntityCompanyQuestion";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                        redirectPage = "ConnectedEntityControlCondition";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual:
                        redirectPage = "";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        redirectPage = "ConnectedEntityControlCondition";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                        redirectPage = "ConnectedEntityControlCondition";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust:
                        redirectPage = "";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        redirectPage = "ConnectedEntityControlCondition";
                        break;
                }
                break;
        }

        return redirectPage;
    }
}