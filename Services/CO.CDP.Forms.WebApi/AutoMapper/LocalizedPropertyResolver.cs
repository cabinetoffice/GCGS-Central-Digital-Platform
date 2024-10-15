using AutoMapper;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc.Localization;

namespace CO.CDP.Forms.WebApi.AutoMapper;

public class LocalizedPropertyResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string, string>
{
    private readonly IHtmlLocalizer<FormsEngineResource> _localizer;

    public LocalizedPropertyResolver(IHtmlLocalizer<FormsEngineResource> localizer)
    {
        _localizer = localizer;
    }

    public string Resolve(TSource source, TDestination destination, string sourceMember, string destMember, ResolutionContext context)
    {
        if (!string.IsNullOrEmpty(sourceMember))
        {
            return _localizer[sourceMember].Value;
        }

        return sourceMember;
    }
}