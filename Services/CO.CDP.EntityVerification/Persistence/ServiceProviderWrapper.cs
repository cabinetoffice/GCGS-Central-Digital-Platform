namespace CO.CDP.EntityVerification.Persistence;

public class ServiceProviderWrapper : IServiceProviderWrapper
{
    public EntityVerificationContext GetRequiredService(IServiceProvider sp)
    {
        return sp.GetRequiredService<EntityVerificationContext>();
    }
}