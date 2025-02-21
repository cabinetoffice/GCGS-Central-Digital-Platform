using CO.CDP.Localization;
using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementAddressModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    [Display(Name = "Forms_FormElementAddress_AddressLine1_Label", ResourceType = typeof(StaticTextResource))]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [Display(Name = "Forms_FormElementAddress_TownOrCity_Label", ResourceType = typeof(StaticTextResource))]
    public string? TownOrCity { get; set; }

    [BindProperty]
    public string? Postcode { get; set; }

    [BindProperty]
    [Display(Name = "Forms_FormElementAddress_Country_Label", ResourceType = typeof(StaticTextResource))]
    public string? Country { get; set; }

    public string UkOrNonUk { get; set; } = "uk";

    public bool IsNonUkAddress => UkOrNonUk == "non-uk";

    public string PostcodeLabel => IsNonUkAddress
        ? StaticTextResource.Forms_FormElementAddress_PostalOrZipCode_Label
        : StaticTextResource.Forms_FormElementAddress_Postcode_Label;

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

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(AddressLine1) ? null :
            new FormAnswer
            {
                AddressValue = new Address
                {
                    AddressLine1 = AddressLine1,
                    TownOrCity = TownOrCity!,
                    Postcode = Postcode,
                    CountryName = CountryName!,
                    Country = Country!,
                }
            };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.AddressValue != null && (
            (IsNonUkAddress && answer.AddressValue.Country != Constants.Country.UKCountryCode)
            || (!IsNonUkAddress && answer.AddressValue.Country == Constants.Country.UKCountryCode)))
        {
            AddressLine1 = answer.AddressValue.AddressLine1;
            TownOrCity = answer.AddressValue.TownOrCity;
            Postcode = answer.AddressValue.Postcode;
            Country = answer.AddressValue.Country;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (CurrentFormQuestionType == FormQuestionType.Address && IsRequired == true)
        {
            if (string.IsNullOrWhiteSpace(AddressLine1))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementAddress_AddressLine1_RequiredError, [nameof(AddressLine1)]);
            }

            if (string.IsNullOrWhiteSpace(TownOrCity))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementAddress_TownOrCity_RequiredError, [nameof(TownOrCity)]);
            }

            if (!IsNonUkAddress && string.IsNullOrWhiteSpace(Postcode))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementAddress_Postcode_RequiredError, [nameof(Postcode)]);
            }

            if (string.IsNullOrWhiteSpace(Country))
            {
                yield return new ValidationResult(StaticTextResource.Forms_FormElementAddress_Country_RequiredError, [nameof(Country)]);
            }
            else
            {
                var valid = Country == Constants.Country.UKCountryCode || Constants.Country.NonUKCountries.ContainsKey(Country);
                if (!valid)
                {
                    yield return new ValidationResult(string.Format(StaticTextResource.Forms_FormElementAddress_InvalidCountryError, Country), [nameof(Country)]);
                }
            }
        }
    }
}
