namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record BulkCreateOrganisations(
    IEnumerable<CreateOrganisation> Organisations);

public record BulkAddMembers(
    IEnumerable<AddMember> Members);

public record BulkAssignUsers(
    IEnumerable<CreateUserAssignment> Assignments);

public record BulkOperationResult(
    int TotalRequested,
    int Succeeded,
    int Failed,
    IEnumerable<BulkItemError> Errors);

public record BulkItemError(
    int Index,
    string Error);
