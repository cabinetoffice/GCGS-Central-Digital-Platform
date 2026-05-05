namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class ReportCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public required string TaxonomyType { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<CategoryPermission> Permissions { get; set; } = new List<CategoryPermission>();
    public ICollection<ReportCategoryAssignment> Assignments { get; set; } = new List<ReportCategoryAssignment>();
}
