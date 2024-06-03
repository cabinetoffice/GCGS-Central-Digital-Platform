using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.OrganisationApp.Pages.Registration;

[AttributeUsage(AttributeTargets.Class)]
public class ValidateRegistrationStepAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (context.HandlerInstance is RegistrationStepModel model)
        {
            if (!model.ValidateStep())
            {
                context.Result = new RedirectResult(model.ToRedirectPageUrl);
            }
        }
        else
        {
            throw new Exception($"{context.ActionDescriptor.ModelTypeInfo?.Name} is not inherited from {nameof(RegistrationStepModel)} class.");
        }
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }
}