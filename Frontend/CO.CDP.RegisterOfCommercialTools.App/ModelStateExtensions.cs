using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CO.CDP.RegisterOfCommercialTools.App;

public static class ModelStateExtensions
{
    public static bool HasError(this ModelStateDictionary modelState, string key)
    {
        return modelState.TryGetValue(key, out var entry) && entry.Errors.Any();
    }

    public static string? GetError(this ModelStateDictionary modelState, string key)
    {
        return modelState.TryGetValue(key, out var entry) ? entry.Errors.FirstOrDefault()?.ErrorMessage : null;
    }
}

