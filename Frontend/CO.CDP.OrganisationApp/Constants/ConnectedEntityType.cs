using WebApiClient = CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public enum ConnectedEntityType
{
    Organisation = 1,
    Individual,
    TrustOrTrustee
}

public static class ConnectedEntityTypeExtensions
{
    public static WebApiClient.ConnectedEntityType AsApiClientConnectedEntityType(this ConnectedEntityType connectedEntityType)
    {
        return connectedEntityType switch
        {
            ConnectedEntityType.Organisation => WebApiClient.ConnectedEntityType.Organisation,
            ConnectedEntityType.Individual => WebApiClient.ConnectedEntityType.Individual,
            ConnectedEntityType.TrustOrTrustee => WebApiClient.ConnectedEntityType.TrustOrTrustee,
            _ => WebApiClient.ConnectedEntityType.Organisation,
        };
    }
    public static ConnectedEntityType AsConnectedEntityType(this WebApiClient.ConnectedEntityType connectedEntityType)
    {
        return connectedEntityType switch
        {
            WebApiClient.ConnectedEntityType.Organisation => ConnectedEntityType.Organisation,
            WebApiClient.ConnectedEntityType.Individual => ConnectedEntityType.Individual,
            WebApiClient.ConnectedEntityType.TrustOrTrustee => ConnectedEntityType.TrustOrTrustee,
            _ => ConnectedEntityType.Organisation,
        };
    }

    public static string Description(this ConnectedEntityType connectedEntityType)
    {
        return connectedEntityType switch
        {
            ConnectedEntityType.Organisation => "Organisation",
            ConnectedEntityType.Individual => "Individual",
            ConnectedEntityType.TrustOrTrustee => "Trustee or trust",
            _ => throw new NotImplementedException()
        };
    }
}