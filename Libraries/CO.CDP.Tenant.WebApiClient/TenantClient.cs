namespace CO.CDP.Tenant.WebApiClient;

public interface ITenantClient
{
    /// <exception cref="TenantClientException.ServerError">The server failed to handle the request due to its internal error.</exception>
    /// <exception cref="TenantClientException.ClientError">The server rejected the request due to client error.</exception>
    /// <exception cref="TenantClientException.ClientError.DuplicateTenantException">The tenant already exists.</exception>
    public Task<Tenant> RegisterTenant(NewTenant newTenant);

    /// <exception cref="TenantClientException.ServerError">The server failed to handle the request due to its internal error.</exception>
    public Task<Tenant?> GetTenant(Guid tenantId);
}

public class TenantClientException : Exception
{
    public class ServerError : TenantClientException;

    public class ClientError : TenantClientException
    {
        public class DuplicateTenantException : ClientError;
    }
}

public record NewTenant
{
    public required string Name { get; init; }

    public required TenantContactInfo ContactInfo { get; init; }
}

public record Tenant
{
    public required Guid Id { get; init; }

    public required string Name { get; init; }

    public required TenantContactInfo ContactInfo { get; init; }
}

public record TenantContactInfo
{
    public required string Email { get; init; }

    public required string Phone { get; init; }
}