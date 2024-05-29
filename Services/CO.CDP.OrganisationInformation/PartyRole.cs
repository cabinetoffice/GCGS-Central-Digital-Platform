using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/codelists/#party-role">PartyRole</a>.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PartyRole
{
    Buyer = 1,
    ProcuringEntity = 2,
    Supplier = 3,
    Tenderer = 4,
    Funder = 5,
    Enquirer = 6,
    Payer = 7,
    Payee = 8,
    ReviewBody = 9,
    InterestedParty = 10,
    Consortium = 11
}