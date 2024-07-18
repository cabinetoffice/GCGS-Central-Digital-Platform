namespace CO.CDP.EntityVerification.Ppon;

public class PponService : IPponService
{
    public string GeneratePponId()
    {
        return $"{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
    }
}
