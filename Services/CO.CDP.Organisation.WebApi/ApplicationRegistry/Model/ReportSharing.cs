namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;

public record ReportCategoryAssignmentDto(
    Guid Id,
    Guid ReportId,
    Guid CategoryId,
    string CategoryName,
    Guid AssignedBy,
    DateTimeOffset AssignedAt);

public record AssignReportToCategory(
    Guid CategoryId);
