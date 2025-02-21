using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Pages.Shared;

public class AddressPartialModel : IValidatableObject
{
    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Shared_Address_AddressLine1))]
    [Required(ErrorMessage = nameof(StaticTextResource.Shared_Address_AddressLine1_ErrorMessage))]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Shared_Address_TownOrCity))]
    [Required(ErrorMessage = nameof(StaticTextResource.Shared_Address_TownOrCity_ErrorMessage))]
    public string? TownOrCity { get; set; }

    [BindProperty]
    [PostcodeRequired]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName(nameof(StaticTextResource.Shared_Address_Country))]
    [Required(ErrorMessage = nameof(StaticTextResource.Shared_Address_Country_ErrorMessage))]
    public string? Country { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    public bool IsNonUkAddress => UkOrNonUk == "non-uk";

    public string PostcodeLabel => IsNonUkAddress ? StaticTextResource.Shared_Address_Postcode_NonUk : StaticTextResource.Shared_Address_Postcode;

    public string? CountryName
    {
        get
        {
            if (Country == null) return null;
            if (Country == Constants.Country.UKCountryCode) return Constants.Country.UnitedKingdom;
            if (Constants.Country.NonUKCountries.ContainsKey(Country)) return Constants.Country.NonUKCountries[Country];
            return null;
        }
    }

    public string? Heading { get; set; }

    public string? AddressHint { get; set; }

    public string? NonUkAddressLink { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Country))
        {
            var valid = Country == Constants.Country.UKCountryCode || Constants.Country.NonUKCountries.ContainsKey(Country);
            if (!valid)
            {
                yield return new ValidationResult($"{StaticTextResource.Shared_Address_ValidationErrorMessage} '{Country}'", [nameof(Country)]);
            }
        }
    }

    public class PostcodeRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var address = (AddressPartialModel)validationContext.ObjectInstance;

            if (!address.IsNonUkAddress && string.IsNullOrWhiteSpace(address.Postcode))
            {
                return new ValidationResult(StaticTextResource.Shared_Address_Postcode_ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
