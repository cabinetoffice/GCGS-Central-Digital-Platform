namespace CO.CDP.EntityVerification.Ppon;

public class UuidPponService : IPponService
{
    public string GeneratePponId()
    {
        return $"{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
    }
}