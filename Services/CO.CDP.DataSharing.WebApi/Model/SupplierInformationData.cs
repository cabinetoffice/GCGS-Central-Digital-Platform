using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record SupplierInformationData
{
    [Required] public required Guid FormId { get; init; }
    public List<SupplierFormAnswer> Answers { get; init; } = new List<SupplierFormAnswer>();
    public List<FormQuestion> Questions { get; init; } = new List<FormQuestion>();
}