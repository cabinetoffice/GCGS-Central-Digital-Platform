using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using CO.CDP.Common.Auth;

namespace CO.CDP.Common.Tests.Auth;

public class OneLoginResponseTests
{
    [Fact]
    public void Constructor_Sets_Properties_Correctly()
    {
        var expectedSub = "urn:fdc:gov.uk:2022:56P4CMsGh_-2sVIB2nsNU7mcLZYhYw=";
        var expectedEmail = "test@example.com";
        var expectedEmailVerified = true;
        var expectedPhoneNumber = "01406946277";
        var expectedPhoneNumberVerified = true;
        var expectedUpdatedAt = 1311280970L;

        var oneLoginResponse = new OneLoginResponce(expectedSub, expectedEmail, expectedEmailVerified, expectedPhoneNumber, expectedPhoneNumberVerified, expectedUpdatedAt);

        Assert.Equal(expectedSub, oneLoginResponse.Sub);
        Assert.Equal(expectedEmail, oneLoginResponse.Email);
        Assert.Equal(expectedEmailVerified, oneLoginResponse.EmailVerified);
        Assert.Equal(expectedPhoneNumber, oneLoginResponse.PhoneNumber);
        Assert.Equal(expectedPhoneNumberVerified, oneLoginResponse.PhoneNumberVerified);
        Assert.Equal(expectedUpdatedAt, oneLoginResponse.UpdatedAt);
    }
}
