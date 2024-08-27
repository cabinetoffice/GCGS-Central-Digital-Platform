using CO.CDP.OrganisationApp.Models;
using CO.CDP.OrganisationApp.Pages.Forms;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.Pages.Forms;

public class FormElementAddressModelTest
{
    [Theory]
    [InlineData("non-uk", true)]
    [InlineData("uk", false)]
    public void IsNonUkAddress_ShouldReturnCorrectValue(string ukOrNonUk, bool expected)
    {
        var model = new FormElementAddressModel { UkOrNonUk = ukOrNonUk };
        model.IsNonUkAddress.Should().Be(expected);
    }

    [Theory]
    [InlineData("non-uk", "Postal or Zip code")]
    [InlineData("uk", "Postcode")]
    public void PostcodeLabel_ShouldReturnCorrectLabel(string ukOrNonUk, string expectedLabel)
    {
        var model = new FormElementAddressModel { UkOrNonUk = ukOrNonUk };
        model.PostcodeLabel.Should().Be(expectedLabel);
    }

    [Fact]
    public void GetAnswer_ShouldReturnNull_WhenAddressLine1IsNullOrWhiteSpace()
    {
        var model = new FormElementAddressModel { AddressLine1 = null };
        model.GetAnswer().Should().BeNull();

        model.AddressLine1 = "   ";
        model.GetAnswer().Should().BeNull();
    }

    [Fact]
    public void GetAnswer_ShouldReturnFormAnswer_WhenAddressLine1IsNotNullOrWhiteSpace()
    {
        var model = new FormElementAddressModel
        {
            AddressLine1 = "123 Main St",
            TownOrCity = "Springfield",
            Postcode = "12345",
            Country = "US"
        };
        var answer = model.GetAnswer();
        answer.Should().NotBeNull();
        answer?.AddressValue?.AddressLine1.Should().Be("123 Main St");
        answer?.AddressValue?.TownOrCity.Should().Be("Springfield");
        answer?.AddressValue?.Postcode.Should().Be("12345");
        answer?.AddressValue?.Country.Should().Be("US");
    }

    [Theory]
    [InlineData("non-uk", "India", "IN", "123 Main St", "Springfield", "12345")]
    [InlineData("uk", "United Kingdom", "GB", "456 Elm St", "London", "67890")]
    public void SetAnswer_ShouldSetPropertiesCorrectly(string ukOrNonUk, string countryName, string country, string addressLine1, string townOrCity, string postcode)
    {
        var model = new FormElementAddressModel { UkOrNonUk = ukOrNonUk };
        var answer = new FormAnswer
        {
            AddressValue = new Address
            {
                AddressLine1 = addressLine1,
                TownOrCity = townOrCity,
                Postcode = postcode,
                CountryName = countryName,
                Country = country
            }
        };

        model.SetAnswer(answer);

        model.AddressLine1.Should().Be(addressLine1);
        model.TownOrCity.Should().Be(townOrCity);
        model.Postcode.Should().Be(postcode);
        model.Country.Should().Be(country);
    }

    [Fact]
    public void SetAnswer_ShouldNotSetProperties_WhenCountryMismatch()
    {
        var model = new FormElementAddressModel { UkOrNonUk = "uk" };
        var answer = new FormAnswer
        {
            AddressValue = new Address
            {
                AddressLine1 = "123 Main St",
                TownOrCity = "Springfield",
                Postcode = "12345",
                CountryName = "India",
                Country = "IN"
            }
        };

        model.SetAnswer(answer);

        model.AddressLine1.Should().BeNull();
        model.TownOrCity.Should().BeNull();
        model.Postcode.Should().BeNull();
        model.Country.Should().BeNull();
    }

    [Fact]
    public void Validate_ShouldReturnValidationErrors_WhenRequiredFieldsAreEmpty()
    {
        var model = new FormElementAddressModel
        {
            CurrentFormQuestionType = FormQuestionType.Address,
            IsRequired = true
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, true);

        results.Should().ContainSingle(r => r.ErrorMessage == "Enter your address line 1" && r.MemberNames.Contains(nameof(model.AddressLine1)));
        results.Should().ContainSingle(r => r.ErrorMessage == "Enter your town or city" && r.MemberNames.Contains(nameof(model.TownOrCity)));
        results.Should().ContainSingle(r => r.ErrorMessage == "Enter your postcode" && r.MemberNames.Contains(nameof(model.Postcode)));
        results.Should().ContainSingle(r => r.ErrorMessage == "Enter your country" && r.MemberNames.Contains(nameof(model.Country)));
    }

    [Fact]
    public void Validate_ShouldNotReturnValidationErrors_WhenRequiredFieldsAreFilled()
    {
        var model = new FormElementAddressModel
        {
            CurrentFormQuestionType = FormQuestionType.Address,
            IsRequired = true,
            AddressLine1 = "123 Main St",
            TownOrCity = "Springfield",
            Postcode = "12345",
            Country = "GB"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, true);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ShouldReturnInvalidCountryError_WhenCountryIsInvalid()
    {
        var model = new FormElementAddressModel
        {
            CurrentFormQuestionType = FormQuestionType.Address,
            IsRequired = true,
            AddressLine1 = "123 Main St",
            TownOrCity = "Springfield",
            Postcode = "12345",
            Country = "InvalidCountry"
        };

        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, true);

        results.Should().ContainSingle(r => r.ErrorMessage == "Invalid country name 'InvalidCountry'" && r.MemberNames.Contains(nameof(model.Country)));
    }
}