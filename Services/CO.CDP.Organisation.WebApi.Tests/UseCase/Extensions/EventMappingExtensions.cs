using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Tests.UseCase.Extensions;

using Events = WebApi.Events;

internal static class EventMappingExtensions
{
    internal static Events.Identifier AsEventValue(this OrganisationIdentifier command) => new()
    {
        Id = command.Id,
        LegalName = command.LegalName,
        Scheme = command.Scheme,
        Uri = null
    };

    internal static Events.Identifier AsEventValue(
        this OrganisationInformation.Persistence.Organisation.Identifier identifier) => new()
    {
        Id = identifier.IdentifierId,
        LegalName = identifier.LegalName,
        Scheme = identifier.Scheme,
        Uri = identifier.Uri
    };

    internal static List<Events.Identifier> AsEventValue(this List<OrganisationIdentifier>? command) =>
        command?.Select(i => i.AsEventValue()).ToList() ?? [];

    internal static List<Events.Identifier> AsEventValue(
        this IEnumerable<OrganisationInformation.Persistence.Organisation.Identifier>? command)
        => command?.Select(i => i.AsEventValue()).ToList() ?? [];

    private static Events.Address AsEventValue(this OrganisationAddress command) => new()
    {
        CountryName = command.CountryName,
        Locality = command.Locality,
        PostalCode = command.PostalCode,
        Region = command.Region,
        StreetAddress = command.StreetAddress,
        Type = command.Type.ToString()
    };

    private static Events.Address AsEventValue(
        this OrganisationInformation.Persistence.Organisation.OrganisationAddress address) => new()
    {
        CountryName = address.Address.CountryName,
        Locality = address.Address.Locality,
        PostalCode = address.Address.PostalCode,
        Region = address.Address.Region,
        StreetAddress = address.Address.StreetAddress,
        Type = address.Type.ToString()
    };

    internal static List<Events.Address> AsEventValue(this List<OrganisationAddress>? command) =>
        command?.Select(a => a.AsEventValue()).ToList() ?? [];

    internal static List<Events.Address> AsEventValue(
        this IEnumerable<OrganisationInformation.Persistence.Organisation.OrganisationAddress>? command)
        => command?.Select(a => a.AsEventValue()).ToList() ?? [];

    internal static Events.ContactPoint AsEventValue(this OrganisationContactPoint? command) => new()
    {
        Email = command?.Email,
        Name = command?.Name,
        Telephone = command?.Telephone,
        Url = command?.Url
    };

    internal static Events.ContactPoint AsEventValue(
        this OrganisationInformation.Persistence.Organisation.ContactPoint? contact) => new()
    {
        Email = contact?.Email,
        Name = contact?.Name,
        Telephone = contact?.Telephone,
        Url = contact?.Url
    };

    internal static List<string> AsEventValue(this List<PartyRole> command) =>
        command.Select(r => r.ToString()).Select(r => char.ToLowerInvariant(r[0]) + r[1..]).ToList();
}