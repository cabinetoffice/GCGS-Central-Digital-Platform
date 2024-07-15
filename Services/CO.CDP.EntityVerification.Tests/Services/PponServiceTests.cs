using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Tests.Services;

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
