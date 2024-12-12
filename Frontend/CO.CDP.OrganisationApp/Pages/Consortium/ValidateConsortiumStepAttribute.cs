using CO.CDP.OrganisationApp.Pages.Registration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.OrganisationApp.Pages.Consortium;

public class ValidateConsortiumStepAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (context.HandlerInstance is ConsortiumStepModel model)
        {
            if (!model.ValidateStep())
            {
                context.Result = new RedirectResult(model.ToRedirectPageUrl);
            }
        }
        else
        {
            throw new Exception($"{context.ActionDescriptor.ModelTypeInfo?.Name} is not inherited from {nameof(ConsortiumStartModel)} class.");
        }
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }
}
