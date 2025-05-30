using CO.CDP.OrganisationInformation;

namespace CO.CDP.DataSharing.WebApi.Model;

public record SharedSupplierInformation
{
    public required Guid OrganisationId { get; init; }
    public required OrganisationType OrganisationType { get; init; }
    public required BasicInformation BasicInformation { get; init; }
    public required List<ConnectedEntityInformation> ConnectedPersonInformation { get; init; }
    public required IEnumerable<FormAnswerSetForPdf> FormAnswerSetForPdfs { get; init; }
    public required List<string> AttachedDocuments { get; init; }
    public required IEnumerable<Identifier> AdditionalIdentifiers { get; init; } = [];
    public required string Sharecode { get; init; }
    public required DateTimeOffset? SharecodeSubmittedAt { get; init; }
}