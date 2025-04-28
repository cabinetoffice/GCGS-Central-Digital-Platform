using CO.CDP.OrganisationInformation.Persistence.Forms;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.OrganisationInformation.Persistence.NonEfEntities;

public record SharedConsentNonEf
{
    public required Guid Guid { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public required SubmissionState SubmissionState { get; set; } = SubmissionState.Draft;
    public string? ShareCode { get; set; }

    public required OrganisationNonEf Organisation { get; set; }
    public required FormNonEf Form { get; set; }
    public List<FormAnswerSetNonEf> AnswerSets { get; set; } = [];
}

public record OrganisationNonEf
{
    public required Guid Guid { get; set; }
    public required string Name { get; set; }
    public required OrganisationType Type { get; set; }
    public List<IdentifierNonEf> Identifiers { get; set; } = [];
    public List<AddressNonEf> Addresses { get; set; } = [];
    public List<ContactPointNonEf> ContactPoints { get; set; } = [];
    public List<PartyRole> Roles { get; set; } = [];
    public SupplierInformationNonEf? SupplierInfo { get; set; }
    public List<ConnectedEntityNonEf> ConnectedEntities { get; set; } = [];
}

public class ConnectedEntityNonEf
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required ConnectedEntityType EntityType { get; set; }
    public bool HasCompanyHouseNumber { get; set; }
    public string? CompanyHouseNumber { get; set; }
    public string? OverseasCompanyNumber { get; set; }
    public ConnectedOrganisationNonEf? Organisation { get; set; }
    public ConnectedIndividualTrustNonEf? IndividualOrTrust { get; set; }
    public ICollection<AddressNonEf> Addresses { get; set; } = [];
    public DateTimeOffset? RegisteredDate { get; set; }
    public string? RegisterName { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}

public record ConnectedIndividualTrustNonEf
{
    public required ConnectedEntityIndividualAndTrustCategoryType Category { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public List<ControlCondition> ControlCondition { get; set; } = [];
    public ConnectedPersonType ConnectedType { get; set; }
    public string? ResidentCountry { get; set; }
}

public record ConnectedOrganisationNonEf
{
    public required ConnectedOrganisationCategory Category { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? InsolvencyDate { get; set; }
    public string? RegisteredLegalForm { get; set; }
    public string? LawRegistered { get; set; }
    public List<ControlCondition> ControlCondition { get; set; } = [];
}

public record IdentifierNonEf
{
    public required string Scheme { get; set; }
    public string? IdentifierId { get; set; }
    public required string LegalName { get; set; }
    public required bool Primary { get; set; }
    public string? Uri { get; set; }
}

public record AddressNonEf
{
    public AddressType? Type { get; set; }
    public required string StreetAddress { get; set; }
    public required string Locality { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public required string CountryName { get; set; }
    public required string Country { get; set; }
}

public record ContactPointNonEf
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Url { get; set; }
}

public record LegalFormNonEf
{
    public required bool RegisteredUnderAct2006 { get; set; }
    public required string RegisteredLegalForm { get; set; }
    public required string LawRegistered { get; set; }
    public required DateTimeOffset RegistrationDate { get; set; }
}

public record SupplierInformationNonEf
{
    public SupplierType? SupplierType { get; set; }
    public List<OperationType> OperationTypes { get; set; } = [];
    public bool CompletedRegAddress { get; set; }
    public bool CompletedPostalAddress { get; set; }
    public bool CompletedVat { get; set; }
    public bool CompletedWebsiteAddress { get; set; }
    public bool CompletedEmailAddress { get; set; }
    public bool CompletedOperationType { get; set; }
    public bool CompletedLegalForm { get; set; }
    public bool CompletedConnectedPerson { get; set; }
    public LegalFormNonEf? LegalForm { get; set; }
}

public record FormNonEf
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required bool IsRequired { get; set; } = true;
}

public record FormAnswerSetNonEf
{
    public required int Id { get; set; }
    public required Guid Guid { get; set; }
    public required int SectionId { get; set; }
    public FormSectionNonEf Section { get; set; } = default!;
    public List<FormAnswerNonEf> Answers { get; set; } = [];
}

public class FormSectionNonEf
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required FormSectionType Type { get; set; }
    public List<FormQuestionNonEf> Questions { get; set; } = [];
}

public class FormAnswerNonEf
{
    public required int QuestionId { get; set; }
    public required int FormAnswerSetId { get; set; }
    public bool? BoolValue { get; set; }
    public double? NumericValue { get; set; }
    public DateTime? DateValue { get; set; }
    public DateTime? StartValue { get; set; }
    public DateTime? EndValue { get; set; }
    public string? TextValue { get; set; }
    public string? OptionValue { get; set; }
    public string? JsonValue { get; set; }
    public AddressNonEf? AddressValue { get; set; }
    public required FormQuestionNonEf Question { get; set; }
    public required FormAnswerSetNonEf FormAnswerSet { get; set; }
}

public class FormQuestionNonEf
{
    public required int Id { get; set; }
    public required Guid Guid { get; set; }
    public required int SortOrder { get; set; } = 0;
    public required FormQuestionType Type { get; set; }
    public required bool IsRequired { get; set; } = true;
    public required string Name { get; set; }
    public required string Title { get; set; }
    public string? SummaryTitle { get; set; }
    public required string? Description { get; set; } = null;
    public required FormQuestionOptions Options { get; set; } = new FormQuestionOptions();
    public required FormSectionNonEf Section { get; set; }
}