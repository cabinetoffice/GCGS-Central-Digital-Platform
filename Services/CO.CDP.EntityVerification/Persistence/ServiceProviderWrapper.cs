namespace CO.CDP.EntityVerification.Persistence;

public class ServiceProviderWrapper : IServiceProviderWrapper
{
    public EntityValidationContext GetRequiredService(IServiceProvider sp)
    {
        return sp.GetRequiredService<EntityValidationContext>();
    }
}