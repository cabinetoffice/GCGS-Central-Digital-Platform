using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Resources;

namespace CO.CDP.Mvc.Validation;
public static class ErrorMessageResolver
{
    public static string? GetErrorMessage(string? errorMessage,
                                          string? errorMessageResourceName,
                                          Type? errorMessageResourceType)
    {
        if (!string.IsNullOrEmpty(errorMessageResourceName) && errorMessageResourceType != null)
        {
            var resourceManagerProperty = errorMessageResourceType?.GetProperty("ResourceManager", BindingFlags.Static | BindingFlags.Public);
            if (resourceManagerProperty == null)
            {
                throw new InvalidOperationException($"No ResourceManager found in '{errorMessageResourceType?.FullName}'.");
            }

            var resourceManager = resourceManagerProperty.GetValue(null) as ResourceManager;

            if (resourceManager == null)
            {
                throw new InvalidOperationException($"No ResourceManager found in '{errorMessageResourceType?.FullName}'.");
            }

            return resourceManager.GetString(errorMessageResourceName);
        }

        if (!string.IsNullOrEmpty(errorMessage))
            return errorMessage;

        // If we've failed to find an appropriate error message we return null
        // It is then the responsibility of the validator which is calling this to make a fallback message which makes sense in context
        return null;
    }
}

