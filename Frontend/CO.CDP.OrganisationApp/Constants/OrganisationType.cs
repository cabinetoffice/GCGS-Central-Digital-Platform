using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public enum OrganisationType
{
    Buyer = 1,
    Supplier = 2
}

public static class OrganisationTypeExtensions
{
    public static PartyRole AsPartyRole(this OrganisationType organisationType)
    {
        switch (organisationType)
        {
            case OrganisationType.Buyer: return PartyRole.Buyer;
            case OrganisationType.Supplier: return PartyRole.Supplier;
            default: return PartyRole.Supplier;
        }
    }
}