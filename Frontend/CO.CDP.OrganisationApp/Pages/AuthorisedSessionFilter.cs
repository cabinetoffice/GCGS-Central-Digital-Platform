using CO.CDP.OrganisationApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CO.CDP.OrganisationApp.Pages;

[AttributeUsage(AttributeTargets.Class)]
public class AuthorisedSessionFilter(ISession session) : Attribute, IPageFilter
{
    public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
    {
    }

    public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
    {
        if (IsDefined(context.HandlerInstance.GetType(), typeof(AuthorisedSessionNotRequiredAttribute)))
        {
            return;
        }

        if (context.HttpContext.User.Identity?.IsAuthenticated == false)
        {
            context.Result = new RedirectResult("/");
        }

        var details = session.Get<UserDetails>(Session.UserDetailsKey);

        if (details == null)
        {
            context.Result = new RedirectResult("/");
        }
    }

    public void OnPageHandlerSelected(PageHandlerSelectedContext context)
    {
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class AuthorisedSessionNotRequiredAttribute : Attribute
{
}