namespace CO.CDP.UserManagement.Shared.Enums;

/// <summary>
/// Party roles that an organisation can hold.
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/codelists/#party-role">PartyRole</a>.
/// Integer values match <c>CO.CDP.UserManagement.Core.Constants.PartyRole</c> and <c>CO.CDP.Organisation.WebApiClient.PartyRole</c>.
/// </summary>
public enum PartyRole
{
    Buyer = 0,
    ProcuringEntity = 1,
    Supplier = 2,
    Tenderer = 3,
    Funder = 4,
    Enquirer = 5,
    Payer = 6,
    Payee = 7,
    ReviewBody = 8,
    InterestedParty = 9
}
