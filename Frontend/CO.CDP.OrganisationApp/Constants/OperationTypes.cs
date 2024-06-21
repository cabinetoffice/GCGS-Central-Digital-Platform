using System.ComponentModel.DataAnnotations;
using CO.CDP.Organisation.WebApiClient;

namespace CO.CDP.OrganisationApp.Constants;

public static class OperationTypeExtensions
{
    public static string Description(this OperationType operationType)
    {
        return operationType switch
        {
            OperationType.SmallorMediumSized => "As a small or medium-sized enterprise",
            OperationType.NonGovernmental => "As a non-governmental organisation that is value-driven and which principally reinvests its surpluses to further social, environmental or cultural objectives",
            OperationType.SupportedEmploymentProvider => "As a supported employment provider",
            OperationType.PublicService => "As a public service mutual",
            OperationType.None => "My organisation is none of the above",
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
                return new ValidationResult("You cannot select 'My organisation is none of the above' along with other options.");
            }
        }

        return ValidationResult.Success!;
    }
}