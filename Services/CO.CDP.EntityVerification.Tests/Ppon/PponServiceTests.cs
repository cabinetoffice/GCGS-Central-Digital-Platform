using CO.CDP.EntityVerification.Ppon;
using CO.CDP.OrganisationInformation;
using System.Text.RegularExpressions;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class PponServiceTests
{
    [Fact]
    public void GeneratePponId_CreatesNewPponIdForNonConsortium_ReturnsPponIdWithValidLength()
    {
        PponService ppon = new PponService();
        string pponId = ppon.GeneratePponId(OrganisationType.Organisation);
        string pattern = @"^P[A-Z]{3}-\d{4}-[A-Z]{4}$";
        bool isMatch = Regex.IsMatch(pponId, pattern);

        Assert.True(isMatch);
    }

    [Fact]
    public void GeneratePponId_CreatesNewPponIdForConsortium_ReturnsPponIdWithValidLengthWithLastCharAsC()
    {
        PponService ppon = new PponService();
        string pponId = ppon.GeneratePponId(OrganisationType.InformalConsortium);
        string pattern = @"^P[A-Z]{3}-\d{4}-[A-Z]{3}C$";
        bool isMatch = Regex.IsMatch(pponId, pattern);

        Assert.True(isMatch);
    }

}