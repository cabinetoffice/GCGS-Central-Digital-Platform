using CO.CDP.EntityVerification.Ppon;

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
        var idLength = 17;

        string pponId = ppon.GeneratePponId();

        Assert.Equal(idLength, pponId.Length);
    }
}