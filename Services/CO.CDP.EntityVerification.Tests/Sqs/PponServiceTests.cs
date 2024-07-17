using CO.CDP.EntityVerification.Sqs;

namespace CO.CDP.EntityVerification.Tests.Sqs;

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
