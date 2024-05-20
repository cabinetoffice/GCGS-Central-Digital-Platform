using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

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