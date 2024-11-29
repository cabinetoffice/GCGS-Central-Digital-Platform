using CO.CDP.Mvc.Validation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.OrganisationApp.Pages;

public class SanitisedInputPageFilter : IPageFilter
{
    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        var model = context.ModelState.Values.FirstOrDefault();
        if (model != null)
        {
            var properties = model.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.GetValue(model) is string value)
                {
                    property.SetValue(model, value.StripAndRemoveObscureWhitespace2());
                }
            }
        }
    }

    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
        throw new NotImplementedException();
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
        throw new NotImplementedException();
    }
}