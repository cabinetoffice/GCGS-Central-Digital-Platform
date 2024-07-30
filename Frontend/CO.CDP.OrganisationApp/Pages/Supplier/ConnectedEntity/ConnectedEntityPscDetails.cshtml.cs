using CO.CDP.Mvc.Validation;
using CO.CDP.OrganisationApp.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CO.CDP.OrganisationApp.Pages.Supplier;

public class ConnectedEntityPscDetailsModel(ISession session) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? ConnectedEntityId { get; set; }

    [BindProperty]
    [DisplayName("First name")]
    [Required(ErrorMessage = "Enter first name")]
    public string? FirstName { get; set; }

    [BindProperty]
    [DisplayName("Last name")]
    [Required(ErrorMessage = "Enter last name")]
    public string? LastName { get; set; }

    [BindProperty]
    [RequiredIf(nameof(ShowDobAndNationality), true, ErrorMessage = "Date of birth must include a day")]
    [RegularExpression(RegExPatterns.Day, ErrorMessage = "Day must be a valid number")]
    public string? Day { get; set; }

    [BindProperty]
    [RequiredIf(nameof(ShowDobAndNationality), true, ErrorMessage = "Date of birth must include a month")]
    [RegularExpression(RegExPatterns.Month, ErrorMessage = "Month must be a valid number")]
    public string? Month { get; set; }

    [BindProperty]
    [RequiredIf(nameof(ShowDobAndNationality), true, ErrorMessage = "Date of birth must include a year")]
    [RegularExpression(RegExPatterns.Year, ErrorMessage = "Year must be a valid number")]
    public string? Year { get; set; }

    [BindProperty]
    public string? DateOfBirth { get; set; }

    [BindProperty]
    [DisplayName("Nationality")]
    [RequiredIf(nameof(ShowDobAndNationality), true, ErrorMessage = "Enter your nationality")]
    public string? Nationality { get; set; }

    [BindProperty]
    public ConnectedEntityType? ConnectedEntityType { get; set; }
    public string? Caption { get; set; }

    public string? Heading { get; set; }

    public string? BackPageLink { get; set; }

    [BindProperty]
    public bool? ShowDobAndNationality { get; set; }
    [BindProperty]
    public bool? RedirectToCheckYourAnswer { get; set; }

    public IActionResult OnGet()
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

        InitModal(state, true);

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

        ModelState.Clear();

        var validationResult = ValidateModel();
        if (!validationResult.isValid)
        {
            foreach (var error in validationResult.errors)
            {
                ModelState.AddModelError(error.Key, error.Value);
            }
            return Page();
        }

        if (ShowDobAndNationality == true)
        {
            var dateString = $"{Year}-{Month!.PadLeft(2, '0')}-{Day!.PadLeft(2, '0')}";
            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                ModelState.AddModelError(nameof(DateOfBirth), "Date of birth must be a real date");
                return Page();
            }

            if (parsedDate > DateTime.Today)
            {
                ModelState.AddModelError(nameof(DateOfBirth), "Date of birth must be today or in the past");
                return Page();
            }

            state.DateOfBirth = new DateTimeOffset(parsedDate, TimeSpan.FromHours(0));
        }

        state.FirstName = FirstName;
        state.LastName = LastName;
        state.Nationality = Nationality;

        session.Set(Session.ConnectedPersonKey, state);

        var checkAnswerPage = (state.ConnectedEntityType == Constants.ConnectedEntityType.Organisation
            ? "ConnectedEntityCheckAnswersOrganisation"
            : "ConnectedEntityCheckAnswersIndividualOrTrust");

        var redirectPage = (RedirectToCheckYourAnswer == true
                        ? checkAnswerPage
                        : GetRedirectLinkPageName(state));

        return RedirectToPage(redirectPage, new { Id, ConnectedEntityId, AddressType = AddressType.Registered, UkOrNonUk = "uk" });
    }

    private void InitModal(ConnectedEntityState state, bool reset = false)
    {
        Caption = state.GetCaption();
        Heading = $"Enter the person with significant control's details";
        ConnectedEntityType = state.ConnectedEntityType;
        ShowDobAndNationality = (state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual
                                || state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual
                                || state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust
                                || state.ConnectedEntityIndividualAndTrustCategoryType == ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust);
        if (reset)
        {
            FirstName = state.FirstName;
            LastName = state.LastName;
            Nationality = state.Nationality;

            if (state.DateOfBirth.HasValue)
            {
                Day = state.DateOfBirth.Value.Day.ToString();
                Month = state.DateOfBirth.Value.Month.ToString();
                Year = state.DateOfBirth.Value.Year.ToString();
            }
        }
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

    private (bool isValid, Dictionary<string, string> errors) ValidateModel()
    {
        var errors = new Dictionary<string, string>();
        bool isValid = true;

        if (string.IsNullOrEmpty(FirstName))
        {
            isValid = false;
            errors[nameof(FirstName)] = "Enter first name";
        }

        if (string.IsNullOrEmpty(LastName))
        {
            isValid = false;
            errors[nameof(LastName)] = "Enter last name";
        }

        if (ShowDobAndNationality == true)
        {
            if (string.IsNullOrEmpty(Day))
            {
                isValid = false;
                errors[nameof(Day)] = "Date of birth must include a day";
            }
            else if (!IsValidPattern(Day, RegExPatterns.Day))
            {
                isValid = false;
                errors[nameof(Day)] = "Day must be a valid number";
            }

            if (string.IsNullOrEmpty(Month))
            {
                isValid = false;
                errors[nameof(Month)] = "Date of birth must include a month";
            }
            else if (!IsValidPattern(Month, RegExPatterns.Month))
            {
                isValid = false;
                errors[nameof(Month)] = "Month must be a valid number";
            }

            if (string.IsNullOrEmpty(Year))
            {
                isValid = false;
                errors[nameof(Year)] = "Date of birth must include a year";
            }
            else if (!IsValidPattern(Year, RegExPatterns.Year))
            {
                isValid = false;
                errors[nameof(Year)] = "Year must be a valid number";
            }

            if (string.IsNullOrEmpty(Nationality))
            {
                isValid = false;
                errors[nameof(Nationality)] = "Enter your nationality";
            }
        }

        return (isValid, errors);
    }
    private bool IsValidPattern(string value, string pattern)
    {
        Regex regex = new Regex(pattern);
        return regex.IsMatch(value);
    }

    private string GetRedirectLinkPageName(ConnectedEntityState state)
    {
        var redirectPage = "";
        switch (state.ConnectedEntityType)
        {
            case Constants.ConnectedEntityType.Organisation:
                break;
            case Constants.ConnectedEntityType.Individual:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForIndividual:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForIndividual:
                        redirectPage = "ConnectedEntityAddress";
                        break;
                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForIndividual:
                        redirectPage = "ConnectedEntityDirectorResidency";
                        break;
                }
                break;
            case Constants.ConnectedEntityType.TrustOrTrustee:
                switch (state.ConnectedEntityIndividualAndTrustCategoryType)
                {
                    case ConnectedEntityIndividualAndTrustCategoryType.PersonWithSignificantControlForTrust:
                    case ConnectedEntityIndividualAndTrustCategoryType.AnyOtherIndividualWithSignificantInfluenceOrControlForTrust:
                        redirectPage = "ConnectedEntityAddress";
                        break;

                    case ConnectedEntityIndividualAndTrustCategoryType.DirectorOrIndividualWithTheSameResponsibilitiesForTrust:
                        break;
                }
                break;
        }

        return redirectPage;
    }
        
}