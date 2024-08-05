using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Pages.Forms;

public class FormElementAddressModel : FormElementModel, IValidatableObject
{
    [BindProperty]
    [DisplayName("Address line 1")]
    public string? AddressLine1 { get; set; }

    [BindProperty]
    [DisplayName("Town or city")]
    public string? TownOrCity { get; set; }

    [BindProperty]
    public string? Postcode { get; set; }

    [BindProperty]
    [DisplayName("Country")]
    public string? Country { get; set; }

    public string UkOrNonUk { get; set; } = "uk";

    public bool IsNonUkAddress => UkOrNonUk == "non-uk";

    public string PostcodeLabel => IsNonUkAddress ? "Postal or Zip code" : "Postcode";

    public override FormAnswer? GetAnswer()
    {
        return string.IsNullOrWhiteSpace(AddressLine1) ? null :
            new FormAnswer
            {
                AddressValue = new Address
                {
                    AddressLine1 = AddressLine1,
                    TownOrCity = TownOrCity!,
                    Postcode = Postcode!,
                    Country = Country!
                }
            };
    }

    public override void SetAnswer(FormAnswer? answer)
    {
        if (answer?.AddressValue != null && (
            (IsNonUkAddress && answer.AddressValue.Country != Constants.Country.UnitedKingdom)
            || (!IsNonUkAddress && answer.AddressValue.Country == Constants.Country.UnitedKingdom)))
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
                yield return new ValidationResult("Enter your address line 1", [nameof(AddressLine1)]);
            }

            if (string.IsNullOrWhiteSpace(TownOrCity))
            {
                yield return new ValidationResult("Enter your town or city", [nameof(TownOrCity)]);
            }

            if (string.IsNullOrWhiteSpace(Postcode))
            {
                yield return new ValidationResult(IsNonUkAddress ? "Enter your postal or zip code" : "Enter your postcode", [nameof(Postcode)]);
            }

            if (string.IsNullOrWhiteSpace(Country))
            {
                yield return new ValidationResult("Enter your country", [nameof(Country)]);
            }
            else
            {
                var valid = Country == Constants.Country.UnitedKingdom || Constants.Country.NonUKCountries.Contains(Country);
                if (!valid)
                {
                    yield return new ValidationResult($"Invalid country name '{Country}'", [nameof(Country)]);
                }
            }
        }
    }
}