using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections;
using System.Reflection;
using System.Resources;

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

        return new List<ModelValidationResult>
        {
            new ModelValidationResult(context.ModelMetadata.PropertyName, GetErrorMessage())
        };
    }

    private string? GetErrorMessage()
    {
        if(ErrorMessage != null)
        {
            return ErrorMessage;
        }

        if(ErrorMessageResourceName == null)
        {
            throw new InvalidOperationException("No Resource specified.");
        }

        var resourceManagerProperty = ErrorMessageResourceType?.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public);
        if (resourceManagerProperty == null)
        {
            throw new InvalidOperationException($"No ResourceManager found in '{ErrorMessageResourceType?.FullName}'.");
        }

        var resourceManager = resourceManagerProperty.GetValue(null) as ResourceManager;

        if (resourceManager == null)
        {
            throw new InvalidOperationException($"No ResourceManager found in '{ErrorMessageResourceType?.FullName}'.");
        }

        return resourceManager.GetString(ErrorMessageResourceName);
    }
}