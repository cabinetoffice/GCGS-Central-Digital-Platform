using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Mvc.Validation.Tests;
public class ValidEmailAddressAttributeTests
{
    private readonly ValidEmailAddressAttribute _validEmailAddressAttribute = new();

    [Theory]
    [InlineData("emailAddress@domain.com", true)]
    [InlineData("emailAddress@domain.COM", true)]
    [InlineData("firstname.lastname@domain.com", true)]
    [InlineData("firstname.o'lastname@domain.com", true)]
    [InlineData("emailAddress@subdomain.domain.com", true)]
    [InlineData("firstname+lastname@domain.com", true)]
    [InlineData("1234567890@domain.com", true)]
    [InlineData("emailAddress@domain-one.com", true)]
    [InlineData("_______@domain.com", true)]
    [InlineData("emailAddress@domain.name", true)]
    [InlineData("emailAddress@domain.superlongtld", true)]
    [InlineData("emailAddress@domain.co.jp", true)]
    [InlineData("firstname-lastname@domain.com", true)]
    [InlineData("info@german-financial-services.vermögensberatung", true)]
    [InlineData("info@german-financial-services.reallylongarbitrarytldthatiswaytoohugejustincase", true)]
    [InlineData("japanese-info@例え.テスト", true)]
    [InlineData("emailAddress@double--hyphen.com", true)]
    public void IsValid_ShouldSucceed(string? email, bool isSuccessExpected)
    {
        Validate(email, isSuccessExpected);
    }

    [Theory]
    [InlineData("emailAddress@123.123.123.123", false)]
    [InlineData("emailAddress@[123.123.123.123]", false)]
    [InlineData("plainaddress", false)]
    [InlineData("@no-local-part.com", false)]
    [InlineData("Outlook Contact <outlook-contact@domain.com>", false)]
    [InlineData("no-at.domain.com", false)]
    [InlineData("no-tld@domain", false)]
    [InlineData(";beginning-semicolon@domain.co.uk", false)]
    [InlineData("middle-semicolon@domain.co;uk", false)]
    [InlineData("trailing-semicolon@domain.com;", false)]
    [InlineData("\"emailAddress+leading-quotes@domain.com", false)]
    [InlineData("emailAddress+middle\"-quotes@domain.com", false)]
    [InlineData("\"quoted-local-part\"@domain.com", false)]
    [InlineData("\"quoted@domain.com\"", false)]
    [InlineData("lots-of-dots@domain..gov..uk", false)]
    [InlineData("two-dots..in-local@domain.com", false)]
    [InlineData("multiple@domains@domain.com", false)]
    [InlineData("spaces in local@domain.com", false)]
    [InlineData("spaces-in-domain@dom ain.com", false)]
    [InlineData("underscores-in-domain@dom_ain.com", false)]
    [InlineData("pipe-in-domain@example.com|gov.uk", false)]
    [InlineData("comma,in-local@gov.uk", false)]
    [InlineData("comma-in-domain@domain,gov.uk", false)]
    [InlineData("pound-sign-in-local£@domain.com", false)]
    [InlineData("local-with-’apostrophe@domain.com", false)]
    [InlineData("local-with-”-quotes@domain.com", false)]
    [InlineData("domain-starts-with-a-dot@.domain.com", false)]
    [InlineData("brackets(in)local@domain.com", false)]
    [InlineData("emailAddress-too-long-aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/" +
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/" +
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa/" +
        "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa@example.com", false)]
    [InlineData("incorrect-punycode@xn---something.com", false)]
    public void IsValid_ShouldFail(string? email, bool isSuccessExpected)
    {
        Validate(email, isSuccessExpected);
    }

    private void Validate(string? emailAddress, bool isSuccessExpected)
    {
        var validationContext = new ValidationContext(new object(), null, null);
        var result = _validEmailAddressAttribute.GetValidationResult(emailAddress, validationContext);

        if (isSuccessExpected)
        {
            result.Should().Be(ValidationResult.Success);
        }
        else
        {
            result.Should().NotBe(ValidationResult.Success);
            result.As<ValidationResult>().ErrorMessage.Should().NotBeEmpty();
        }
    }
}
