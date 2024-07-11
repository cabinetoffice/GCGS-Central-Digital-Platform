using CO.CDP.EntityVerification.Services;

namespace CO.CDP.EntityVerification.Tests.Services;

public class PponServiceTests
{
    [Fact]
    public void GeneratePponTest()
    {
        var scheme = "ppon";
        var departmentIdentifier = "gcg";
        PponService ppon = new PponService();
        var idLength = 32;

        string pponId = ppon.GeneratePponId(scheme, departmentIdentifier);

        Assert.Equal(scheme.Length + 1 + idLength + 1 + departmentIdentifier.Length, pponId.Length);
    }
}
