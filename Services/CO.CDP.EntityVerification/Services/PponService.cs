namespace CO.CDP.EntityVerification.Services;

public class PponService : IPponService
{
    public string GeneratePponId(string scheme, string departmentIdentifier)
    {
        var id = Guid.NewGuid().ToString().Replace("-", string.Empty);

        return $"{scheme}-{id}-{departmentIdentifier}";
    }
}
