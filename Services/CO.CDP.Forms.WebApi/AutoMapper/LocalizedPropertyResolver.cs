using AutoMapper;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;

namespace CO.CDP.Forms.WebApi.AutoMapper;

public class LocalizedPropertyResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string, string>
{
    private readonly IStringLocalizer<FormsEngineResource> _localizer;

    public LocalizedPropertyResolver(IStringLocalizer<FormsEngineResource> localizer)
    {
        _localizer = localizer;
    }

    public string Resolve(TSource source, TDestination destination, string sourceMember, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(sourceMember))
        {
            return _localizer[sourceMember];
        }

        return sourceMember;
    }
}