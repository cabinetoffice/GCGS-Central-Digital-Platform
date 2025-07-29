namespace CO.CDP.OrganisationApp.Constants;

/// <summary>
/// Constants for authorization policy names used throughout the application.
/// </summary>
public static class PolicyNames
{
    /// <summary>
    /// Party role policies - require the user's organisation to have the specified party role (approved, not pending).
    /// </summary>
    public static class PartyRole
    {
        public const string Buyer = "PartyRole_Buyer";
        public const string Supplier = "PartyRole_Supplier";
        public const string ProcuringEntity = "PartyRole_ProcuringEntity";
        public const string Tenderer = "PartyRole_Tenderer";
        public const string Funder = "PartyRole_Funder";
        public const string Enquirer = "PartyRole_Enquirer";
        public const string Payer = "PartyRole_Payer";
        public const string Payee = "PartyRole_Payee";
        public const string ReviewBody = "PartyRole_ReviewBody";
        public const string InterestedParty = "PartyRole_InterestedParty";

        /// <summary>
        /// Policy that requires the user to be an approved buyer who has signed the latest MoU.
        /// Combines party role validation and MoU signature verification.
        /// </summary>
        public const string BuyerWithSignedMou = "PartyRole_BuyerSignedMou";
    }
}