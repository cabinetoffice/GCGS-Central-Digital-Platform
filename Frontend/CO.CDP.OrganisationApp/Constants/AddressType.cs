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
        return addressType switch
        {
            AddressType.Registered => WebApiClient.AddressType.Registered,
            AddressType.Postal => WebApiClient.AddressType.Postal,
            _ => WebApiClient.AddressType.Registered,
        };
    }
}