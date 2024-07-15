namespace CO.CDP.EntityVerification.Services;

public class PponService : IPponService
{
    public string GeneratePponId()
    {
        var id = Guid.NewGuid().ToString().Replace("-", string.Empty);

        return $"{id}";
    }
}
