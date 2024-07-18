using CO.CDP.EntityVerification.Ppon;

namespace CO.CDP.EntityVerification.Tests.Ppon;

public class PponServiceTests
{
    [Fact]
    public void GeneratePponId_CreatesNewPponId_ReturnsPponIdWithValidLength()
    {
        PponService ppon = new PponService();
        var idLength = 32;

        string pponId = ppon.GeneratePponId();

        Assert.Equal(idLength, pponId.Length);
    }
}
