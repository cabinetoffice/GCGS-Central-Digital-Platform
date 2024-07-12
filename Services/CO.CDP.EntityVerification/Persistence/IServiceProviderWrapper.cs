namespace CO.CDP.EntityVerification.Persistence;

public interface IServiceProviderWrapper
{
    EntityValidationContext GetRequiredService(IServiceProvider sp);
}
