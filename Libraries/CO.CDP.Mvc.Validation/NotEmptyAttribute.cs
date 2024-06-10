using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections;

namespace CO.CDP.Mvc.Validation;

public class NotEmptyAttribute : Attribute, IModelValidator
{
    public required string ErrorMessage { get; set; }

    public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
    {
        var collection = context.Model as ICollection;
        if (collection != null && collection.Count > 0)
        {
            return [];
        }

        return new List<ModelValidationResult>
        {
            new ModelValidationResult(context.ModelMetadata.PropertyName, ErrorMessage)
        };
    }
}