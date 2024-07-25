using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CO.CDP.OrganisationApp.Constants;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

[Authorize]
public class ConnectedEntityControlConditionModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [NotEmpty(ErrorMessage = "Select the Which specified conditions of control does your organisation have?")]
    public required List<ConnectedEntityControlCondition> ControlConditions { get; set; } = [];

    public string? Caption { get; set; }

    public string? Heading { get; set; }

    public IActionResult OnGet()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state, true);

        return Page();
    }
    public IActionResult OnPost()
    {
        var (valid, state) = ValidatePage();
        if (!valid)
        {
            return RedirectToPage(ConnectedEntityId.HasValue ?
                "ConnectedEntityCheckAnswers" : "ConnectedEntitySupplierCompanyQuestion", new { Id, ConnectedEntityId });
        }

        InitModal(state);

        if (!ModelState.IsValid) return Page();

        state.ControlConditions = ControlConditions;

        session.Set(Session.ConnectedPersonKey, state);

        var redirectPage = GetRedirectLinkPageName(state);
        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId });
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

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        Heading = $"Which specified conditions of control does {state.OrganisationName} have?";

        if (reset)
        {
            ControlConditions = state.ControlConditions;
        }
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
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                    case ConnectedEntityOrganisationCategoryType.DirectorOrTheSameResponsibilities:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ParentOrSubsidiaryCompany:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.ACompanyYourOrganisationHasTakenOver:
                        redirectPage = "";
                        break;
                    case ConnectedEntityOrganisationCategoryType.AnyOtherOrganisationWithSignificantInfluenceOrControl:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                }
                break;
            case ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        redirectPage = "ConnectedEntityCompanyRegistrationDate";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                break;
        }

        return redirectPage;
    }
}