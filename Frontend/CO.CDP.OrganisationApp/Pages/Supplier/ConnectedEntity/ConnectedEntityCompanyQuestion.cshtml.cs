using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityCompanyQuestionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Please select an option")]
    public bool? HasCompaniesHouseNumber { get; set; }

    [BindProperty]
    [RequiredIf(nameof(HasCompaniesHouseNumber), true, ErrorMessage = "Please enter the Companies House number.")]
    public string? CompaniesHouseNumber { get; set; }
    public string? Caption { get; set; }
    public string? Heading { get; set; }
    public string? Hint { get; set; }

    public IActionResult OnGet(bool? selected)
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModal(state);

        HasCompaniesHouseNumber = selected.HasValue ? selected : state.HasCompaniesHouseNumber;
        CompaniesHouseNumber = state.CompaniesHouseNumber;
        
        return Page();
    }

    public IActionResult OnPost()
    {
        var state = session.Get<ConnectedEntityState>(Session.ConnectedPersonKey);
        if (state == null)
        {
            return RedirectToPage("ConnectedEntitySupplierHasControl", new { Id });
        }

        InitModal(state);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        state.HasCompaniesHouseNumber = HasCompaniesHouseNumber;
        state.CompaniesHouseNumber = CompaniesHouseNumber;

        session.Set(Session.ConnectedPersonKey, state);

        var redirectPage = GetRedirectLinkPageName(state);
        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId });
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
                        redirectPage = "ConnectedEntityControlCondition";
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        redirectPage = "ConnectedEntityCheckAnswers";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        redirectPage = "ConnectedEntityCompanyInsolvencyDate";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = "ConnectedEntityControlCondition";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.Individual:
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                break;
        }

        return redirectPage;
    }
    private void InitModal(ConnectedEntityState state)
    {
        Caption = state.GetCaption();
        Heading = $"Is {state.OrganisationName} registered with Companies House?";
        Hint = "Is the ‘connected person’ registered with Companies House as required by the Companies Act 2006.";
    }
}