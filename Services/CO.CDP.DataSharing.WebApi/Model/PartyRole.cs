using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/codelists/#party-role">PartyRole</a>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
internal enum PartyRole
{
    Buyer,
    ProcuringEntity,
    Supplier,
    Tenderer,
    Funder,
    Enquirer,
    Payer,
    Payee,
    ReviewBody,
    InterestedParty,
    Consortium
}