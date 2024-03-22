using CO.CDP.OrganisationApp.CustomeValidationAttributes;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.CustomeValidationAttributes;

public class RequiredIfAttributeTests
{
    public class RequiredIfAttributeValidationTests
    {
        [Fact]
        public void Should_Fail_Validation_When_OrganisationType_Matches_And_IdentificationNumber_IsNull()
        {
            var model = new OrganisationTestModel { OrganisationType = "CCNI" };
            var validationContext = new ValidationContext(model, null, null)
            {
                MemberName = nameof(OrganisationTestModel.CharityCommissionNorthernIrelandNumber)
            };
            var attribute = new RequiredIfAttribute("OrganisationType", "CCNI");

            var result = attribute.GetValidationResult(null, validationContext);

            result.Should().NotBeNull()
                  .And.BeOfType<ValidationResult>()
                  .Which.ErrorMessage.Should().Be("CharityCommissionNorthernIrelandNumber is required");
        }
    }

    [Fact]
    public void Should_Pass_All_Validation_When_OrganisationType_Matches_And_IdentificationNumber_IsProvided()
    {
        var model = new OrganisationTestModel
        {
            OrganisationType = "CCNI",
            CharityCommissionNorthernIrelandNumber = "12345"
        };
        var context = new ValidationContext(model, null, null)
        {
            MemberName = nameof(OrganisationTestModel.CharityCommissionNorthernIrelandNumber)
        };
        var attribute = new RequiredIfAttribute("OrganisationType", "CCNI");

        var result = attribute.GetValidationResult(model.CharityCommissionNorthernIrelandNumber, context);

        result.Should().BeNull("because the identification number is provided when the organisation type matches");
    }

    private class OrganisationTestModel
    {
        public string? OrganisationType { get; set; }
        public string? CharityCommissionNorthernIrelandNumber { get; set; }
    }
}

