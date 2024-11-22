using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections;

namespace CO.CDP.Mvc.Validation;

public class NotEmptyAttribute : Attribute, IModelValidator
{
    public string? ErrorMessage { get; set; }
    public string? ErrorMessageResourceName { get; set; }
    public Type? ErrorMessageResourceType { get; set; }

    public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
    {
        var collection = context.Model as ICollection;
        if (collection != null && collection.Count > 0)
        {
            return [];
        }

        var errorMessage = ErrorMessageResolver.GetErrorMessage(ErrorMessage, ErrorMessageResourceName, ErrorMessageResourceType);

        return new List<ModelValidationResult>
        {
            new ModelValidationResult(context.ModelMetadata.PropertyName, errorMessage ?? $"{context.ModelMetadata.DisplayName} must not be empty")
        };
    }
}