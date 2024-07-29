using WebApiClient = CO.CDP.Organisation.WebApiClient;
namespace CO.CDP.OrganisationApp.Constants;

public enum ConnectedEntityControlCondition
{
    OwnsShares = 1,
    HasVotingRights,
    CanAppointOrRemoveDirectors,
    HasOtherSignificantInfluenceOrControl,
}

public static class ConnectedEntityControlConditionExtensions
{
    public static string Description(this ConnectedEntityControlCondition entityControlCondition)
    {
        return entityControlCondition switch
        {
            ConnectedEntityControlCondition.OwnsShares => "Owns shares",
            ConnectedEntityControlCondition.HasVotingRights => "Has voting rights",
            ConnectedEntityControlCondition.CanAppointOrRemoveDirectors => "Can appoint or remove directors",
            ConnectedEntityControlCondition.HasOtherSignificantInfluenceOrControl => "Has other significant influence or control",
            _ => throw new NotImplementedException()
        };
    }

    public static WebApiClient.ControlCondition AsApiClientControlCondition(this ConnectedEntityControlCondition entityControlCondition)
    {
        switch (entityControlCondition)
        {
            case ConnectedEntityControlCondition.OwnsShares: return WebApiClient.ControlCondition.OwnsShares;
            case ConnectedEntityControlCondition.HasVotingRights: return WebApiClient.ControlCondition.HasVotingRights;
            case ConnectedEntityControlCondition.CanAppointOrRemoveDirectors: return WebApiClient.ControlCondition.CanAppointOrRemoveDirectors;
            case ConnectedEntityControlCondition.HasOtherSignificantInfluenceOrControl: return WebApiClient.ControlCondition.HasOtherSignificantInfluenceOrControl;
            default: return WebApiClient.ControlCondition.OwnsShares;
        }
    }

    public static ConnectedEntityControlCondition AsConnectedEntityClientControlCondition(this WebApiClient.ControlCondition entityControlCondition)
    {
        switch (entityControlCondition)
        {
            case WebApiClient.ControlCondition.OwnsShares: return ConnectedEntityControlCondition.OwnsShares;
            case WebApiClient.ControlCondition.HasOtherSignificantInfluenceOrControl: return ConnectedEntityControlCondition.HasOtherSignificantInfluenceOrControl;
            case WebApiClient.ControlCondition.HasVotingRights: return ConnectedEntityControlCondition.HasVotingRights;
            case WebApiClient.ControlCondition.CanAppointOrRemoveDirectors: return ConnectedEntityControlCondition.CanAppointOrRemoveDirectors;
            default: return ConnectedEntityControlCondition.OwnsShares;
        }
    }

    public static ICollection<WebApiClient.ControlCondition> AsApiClientControlConditionList(this List<ConnectedEntityControlCondition> entityControlConditions)
    {
        var list = new List<WebApiClient.ControlCondition>();

        foreach (var item in entityControlConditions)
        {
            list.Add(item.AsApiClientControlCondition());
        }

        return list;
    }
}