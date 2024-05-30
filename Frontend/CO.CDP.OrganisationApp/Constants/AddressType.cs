using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public enum AddressType
{
    Registered = 1,
    Postal = 2
}

public static class AddressTypeExtensions
{
    public static WebApiClient.AddressType AsApiClientAddressType(this AddressType addressType)
    {
        switch (addressType)
        {
            case AddressType.Registered: return WebApiClient.AddressType.Registered;
            case AddressType.Postal: return WebApiClient.AddressType.Postal;
            default: return WebApiClient.AddressType.Registered;
        }
    }
}