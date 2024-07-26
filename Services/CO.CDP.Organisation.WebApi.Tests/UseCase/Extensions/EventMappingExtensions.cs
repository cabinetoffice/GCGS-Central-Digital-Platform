using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase.Extensions;

internal static class EventMappingExtensions
{
    internal static Events.Identifier AsEventValue(this OrganisationIdentifier command) => new()
    {
        Id = command.Id,
        LegalName = command.LegalName,
        Scheme = command.Scheme,
        Uri = null
    };

    internal static List<Events.Identifier> AsEventValue(this List<OrganisationIdentifier>? command) =>
        command?.Select(i => i.AsEventValue()).ToList() ?? [];

    private static Events.Address AsEventValue(this OrganisationAddress command) => new()
    {
        CountryName = command.CountryName,
        Locality = command.Locality,
        PostalCode = command.PostalCode,
        Region = command.Region,
        StreetAddress = command.StreetAddress,
        Type = command.Type.ToString()
    };

    internal static List<Events.Address> AsEventValue(this List<OrganisationAddress>? command) =>
        command?.Select(a => a.AsEventValue()).ToList() ?? [];

    internal static Events.ContactPoint AsEventValue(this OrganisationContactPoint? command) => new()
    {
        Email = command?.Email,
        Name = command?.Name,
        Telephone = command?.Telephone,
        Url = command?.Url
    };

    internal static List<string> AsEventValue(this List<PartyRole> command) =>
        command.Select(r => r.ToString()).Select(r => char.ToLowerInvariant(r[0]) + r[1..]).ToList();
}