using CO.CDP.EntityVerification.Ppon;
using System.Text.RegularExpressions;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class PponServiceTests
{
    [Fact]
    public void GenerateUuidPponId_CreatesNewPponId_ReturnsPponIdWithValidLength()
    {
        UuidPponService ppon = new UuidPponService();
        var idLength = 32;

        string pponId = ppon.GeneratePponId();

        Assert.Equal(idLength, pponId.Length);
    }

    [Fact]
    public void GeneratePponId_CreatesNewPponId_ReturnsPponIdWithValidLength()
    {
        PponService ppon = new PponService();
        string pponId = ppon.GeneratePponId();
        string pattern = @"^P[A-Z]{3}-\d{4}-[A-Z]{4}$";
        bool isMatch = Regex.IsMatch(pponId, pattern);

        Assert.True(isMatch);
    }
}