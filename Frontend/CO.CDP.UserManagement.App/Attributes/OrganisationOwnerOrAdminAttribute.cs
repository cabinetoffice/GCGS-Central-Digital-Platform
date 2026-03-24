using System;
using Microsoft.AspNetCore.Authorization;
using CO.CDP.UserManagement.App.Constants;

namespace CO.CDP.UserManagement.App.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
public sealed class OrganisationOwnerOrAdminAttribute : AuthorizeAttribute
{
    public OrganisationOwnerOrAdminAttribute()
    {
        Policy = PolicyNames.OrganisationOwnerOrAdmin;
    }
}
