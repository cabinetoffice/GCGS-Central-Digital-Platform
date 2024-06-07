using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.OrganisationApp.Pages;

[AttributeUsage(AttributeTargets.Class)]
public class AuthorisedSessionAttribute : Attribute, IPageFilter
{
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == false)
        {
            context.Result = new RedirectResult("/");
        }

        if (context.HandlerInstance is LoggedInUserAwareModel model)
        {
            if (!model.UserDetailsAvailable)
            {
                context.Result = new RedirectResult("/");
            }
        }
        else
        {
            throw new Exception($"{context.ActionDescriptor.ModelTypeInfo?.Name} is not inherited from {nameof(LoggedInUserAwareModel)} class.");
        }
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }
}