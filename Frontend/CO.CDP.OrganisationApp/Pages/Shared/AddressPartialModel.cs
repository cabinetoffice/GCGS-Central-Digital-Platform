using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Shared;

public class AddressPartialModel : IValidatableObject
{
    [BindProperty]
    [DisplayName("Address line 1")]
    [Required(ErrorMessage = "Enter your address line 1")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName("Town or city")]
    [Required(ErrorMessage = "Enter your town or city")]
    public string? TownOrCity { get; set; }

    [BindProperty]
    [PostcodeRequired]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    [Required(ErrorMessage = "Enter your country")]
    public string? Country { get; set; }

    [BindProperty(SupportsGet = true)]
    public string UkOrNonUk { get; set; } = "uk";

    public bool IsNonUkAddress => UkOrNonUk == "non-uk";

    public string PostcodeLabel => IsNonUkAddress ? "Postal or Zip code" : "Postcode";

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
                yield return new ValidationResult($"Invalid country name '{Country}'", [nameof(Country)]);
            }
        }
    }

    public class PostcodeRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var address = (AddressPartialModel)validationContext.ObjectInstance;

            if (string.IsNullOrWhiteSpace(address.Postcode))
            {
                return new ValidationResult(address.IsNonUkAddress ? "Enter your postal or zip code" : "Enter your postcode");
            }

            return ValidationResult.Success;
        }
    }
}