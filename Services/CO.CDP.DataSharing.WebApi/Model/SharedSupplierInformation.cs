namespace CO.CDP.DataSharing.WebApi.Model;

public record SharedSupplierInformation
{
    public required Guid OrganisationId { get; init; }
    public required BasicInformation BasicInformation { get; init; }
    public required Task<List<ConnectedPersonInformation>> ConnectedPersonInformation { get; init; }
}