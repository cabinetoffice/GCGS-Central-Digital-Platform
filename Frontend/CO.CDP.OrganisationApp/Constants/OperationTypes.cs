using CO.CDP.Localization;
using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;
using System.Resources;

namespace CO.CDP.OrganisationApp.Constants;

public static class OperationTypeExtensions
{
    public static string Description(this OperationType operationType)
    {
        return operationType switch
        {
            OperationType.SmallOrMediumSized => StaticTextResource.Supplier_OperationType_SmallOrMediumSized,
            OperationType.NonGovernmental => StaticTextResource.Supplier_OperationType_NonGovernmental,
            OperationType.SupportedEmploymentProvider => StaticTextResource.Supplier_OperationType_SupportedEmploymentProvider,
            OperationType.PublicService => StaticTextResource.Supplier_OperationType_PublicService,
            OperationType.None => StaticTextResource.Supplier_OperationType_None,
            _ => throw new NotImplementedException()
        };
    }

    public static string ShortDescription(this OperationType operationType)
    {
        return operationType switch
        {
            OperationType.SmallOrMediumSized => StaticTextResource.Supplier_OperationType_SmallOrMediumSized_Short,
            OperationType.NonGovernmental => StaticTextResource.Supplier_OperationType_NonGovernmental_Short,
            OperationType.SupportedEmploymentProvider => StaticTextResource.Supplier_OperationType_SupportedEmploymentProvider_Short,
            OperationType.PublicService => StaticTextResource.Supplier_OperationType_PublicService_Short,
            OperationType.None => StaticTextResource.Supplier_OperationType_None_Short,
            _ => throw new NotImplementedException()
        };
    }
}

public class ValidOperationTypeSelectionAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
    {
        if (value is List<OperationType> operationTypes)
        {
            if (operationTypes.Contains(OperationType.None) && operationTypes.Count > 1)
            {
                return new ValidationResult(StaticTextResource.Supplier_OperationType_ValidationError);
            }
        }

        return ValidationResult.Success!;
    }
}