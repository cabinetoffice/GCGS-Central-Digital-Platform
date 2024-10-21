using AutoMapper;
using CO.CDP.Localization;
using Microsoft.AspNetCore.Mvc.Localization;

namespace CO.CDP.DataSharing.WebApi.AutoMapper;

public class LocalizedPropertyResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string, string>
{
    private readonly IHtmlLocalizer<FormsEngineResource> _localizer;

    public LocalizedPropertyResolver(IHtmlLocalizer<FormsEngineResource> localizer)
    {
        _localizer = localizer;
    }

    public string Resolve(TSource source, TDestination destination, string sourceMember, string destMember, ResolutionContext context)
    {
        return _localizer[sourceMember].Value;
    }
}

public class NullableLocalizedPropertyResolver<TSource, TDestination> : IMemberValueResolver<TSource, TDestination, string?, string?>
{
    private readonly IHtmlLocalizer<FormsEngineResource> _localizer;

    public NullableLocalizedPropertyResolver(IHtmlLocalizer<FormsEngineResource> localizer)
    {
        _localizer = localizer;
    }

    public string? Resolve(TSource? source, TDestination? destination, string? sourceMember, string? destMember, ResolutionContext context)
    {
        if (sourceMember == null)
        {
            return null;
        }

        return _localizer[sourceMember].Value;
    }
}