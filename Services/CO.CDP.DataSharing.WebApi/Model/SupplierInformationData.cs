using System.ComponentModel.DataAnnotations;

namespace CO.CDP.DataSharing.WebApi.Model;

internal record SupplierInformationData
{
    public required Form Form { get; init; }
    public required List<SupplierFormAnswer> Answers { get; init; } = new();
    public required List<FormQuestion> Questions { get; init; } = new();
}