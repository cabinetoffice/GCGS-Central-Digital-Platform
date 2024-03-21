using CO.CDP.OrganisationApp.CustomeValidationAttributes;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.OrganisationApp.Tests.CustomeValidationAttributes
{
    public class RequiredIfAttributeTests
    {
        [Theory]
        [InlineData("CCNI", null, false)]
        [InlineData("CCNI", "", false)]
        [InlineData("CCNI", "123", true)]
        [InlineData("OTHER", "", true)]
        [InlineData("OTHER", null, true)]
        public void Validate_RequiredIfCondition_ReturnsExpectedResult(string organisationType, string? charityNumber, bool isValidExpected)
        {
            var attribute = new RequiredIfAttribute("OrganisationType", "CCNI");
            var model = new
            {
                OrganisationType = organisationType,
                CharityCommissionNorthernIrelandNumber = charityNumber
            };
            var validationContext = new ValidationContext(model)
            {
                MemberName = "CharityCommissionNorthernIrelandNumber"
            };

            var result = attribute.GetValidationResult(model.CharityCommissionNorthernIrelandNumber, validationContext);

            if (isValidExpected)
            {
                result.Should().BeNull();
            }
            else
            {
                result.Should().NotBeNull();
                result.ErrorMessage.Should().Be("CharityCommissionNorthernIrelandNumber is required");
            }
        }
    }
}
