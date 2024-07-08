using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence;

public class ConnectedEntity : IEntityDate
{
    public int Id { get; set; }
    public required ConnectedEntityType EntityType { get; set; }
    public bool HasCompnayHouseNumber { get; set; }
    public string? CompanyHouseNumber { get; set; }
    public string? OverseasCompanyNumber { get; set; }

    public ConnectedOrganisation? Organisation { get; set; }
    public ConnectedIndividualTrust? IndividualOrTrust { get; set; }
    public ICollection<ConnectedEntityAddress> Addresses { get; set; } = [];

    public DateTimeOffset? RegisteredDate { get; set; }
    public string? RegisterName { get; set; }
        
    public required Organisation SupplierOrganisation { get; set; }

    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }


    [Owned]
    public record ConnectedEntityAddress
    {
        public int Id { get; set; }
        public required AddressType Type { get; set; }
        public required Address Address { get; set; }
    }

    [Owned]
    public record ConnectedIndividualTrust : IEntityDate
    {
        public int Id { get; set; }
        public required ConnectedPersonCategory Category { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public ICollection<ControlCondition> ControlCondition { get; set; } = [];
        public ConnectedPersonType ConnectedType { get; set; }
        public Guid? PersonId { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record ConnectedOrganisation : IEntityDate
    {
        public int Id { get; set; }
        public required ConnectedOrganisationCategory Category { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset? InsolvencyDate { get; set; }
        public string? RegisteredLegalForm { get; set; }
        public string? LawRegistered { get; set; }
        public ICollection<ControlCondition> ControlCondition { get; set; } = [];
        public Guid? OrganisationId { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    public enum ControlCondition
    {
        OwnsShares,
        HasVotingRights,
        CanAppointOrRemoveDirectors,
        HasOtherSignificantInfluenceOrControl
    }

    public enum ConnectedEntityType
    {
        Organisation,
        Individual,
        TrustOrTrustee
    }

    public enum ConnectedPersonType
    {
        Individual,
        TrustOrTrustee
    }

    public enum ConnectedPersonCategory
    {
        PersonWithSignificantControl,
        DirectorOrIndividualWithTheSameResponsibilities,
        AnyOtherIndividualWithSignificantInfluenceOrControl
    }

    public enum ConnectedOrganisationCategory
    {
        RegisteredCompany,
        DirectorOrTheSameResponsibilities,
        ParentOrSubsidiaryCompany,
        ACompanyYourOrganisationHasTakenOver,
        AnyOtherOrganisationWithSignificantInfluenceOrControl,
    }

}