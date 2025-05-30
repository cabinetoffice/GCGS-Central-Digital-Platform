using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

public record BasicInformation
{
    public SupplierType? SupplierType { get; init; }
    public Address? RegisteredAddress { get; init; }
    public Address? PostalAddress { get; init; }
    public string? VatNumber { get; init; }
    public string? WebsiteAddress { get; init; }
    public string? EmailAddress { get; init; }
    public string? Role { get; init; }
    public BasicLegalForm? LegalForm { get; init; }
    public string? OrganisationName { get; init; }
    public List<OperationType> OperationTypes { get; init; } = [];
}

public record BasicLegalForm
{
    public bool RegisteredUnderAct2006 { get; init; }
    public string RegisteredLegalForm { get; init; } = string.Empty;
    public string LawRegistered { get; init; } = string.Empty;
    public DateTimeOffset RegistrationDate { get; init; }
}