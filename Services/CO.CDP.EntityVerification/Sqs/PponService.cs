namespace CO.CDP.EntityVerification.Sqs;

public class PponService : IPponService
{
    public string GeneratePponId()
    {
        return $"{Guid.NewGuid().ToString().Replace("-", string.Empty)}";
    }
}
