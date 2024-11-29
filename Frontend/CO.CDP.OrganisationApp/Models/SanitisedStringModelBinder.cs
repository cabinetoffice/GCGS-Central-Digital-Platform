using CO.CDP.OrganisationApp.Extensions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.OrganisationApp.Models;

public class SanitisedStringModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult != ValueProviderResult.None)
        {
            var value = valueProviderResult.FirstValue?.StripAndRemoveObscureWhitespace();
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        return Task.CompletedTask;
    }
}