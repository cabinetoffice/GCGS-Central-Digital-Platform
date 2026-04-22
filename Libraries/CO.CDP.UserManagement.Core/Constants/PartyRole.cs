namespace CO.CDP.UserManagement.Core.Constants;

/// <summary>
/// Party roles that an organisation can hold.
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/codelists/#party-role">PartyRole</a>.
/// Integer values align with <c>CO.CDP.OrganisationInformation.PartyRole</c> (the OI persistence model).
/// </summary>
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
    InterestedParty = 10
}