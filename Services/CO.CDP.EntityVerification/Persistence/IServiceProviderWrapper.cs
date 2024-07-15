namespace CO.CDP.EntityVerification.Persistence;

public interface IServiceProviderWrapper
{
    EntityVerificationContext GetRequiredService(IServiceProvider sp);
}
