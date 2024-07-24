using System.Text.Json.Serialization;
using CO.CDP.OrganisationInformation.Serialisation;

namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/codelists/#party-role">PartyRole</a>.
/// </summary>
[JsonConverter(typeof(LowerCamelCaseEnumConverter<PartyRole>))]
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