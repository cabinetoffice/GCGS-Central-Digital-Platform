namespace CO.CDP.DataSharing.WebApi.Tests.AutoMapper
{
    using System;
    using Xunit;
    using FluentAssertions;
    using Moq;
    using Microsoft.AspNetCore.Mvc.Localization;
    using CO.CDP.Localization;
    using CO.CDP.DataSharing.WebApi.AutoMapper;

    public class LocalizedPropertyResolverTests
    {
        [Fact]
        public void LocalizedPropertyResolver_ResolvesKey()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            localizerMock.Setup(l => l["SomeKey"])
                .Returns(new LocalizedHtmlString("SomeKey", "LocalizedValue"));

            var resolver = new LocalizedPropertyResolver<object, object>(localizerMock.Object);

            var result = resolver.Resolve(
                source: null!,
                destination: null!,
                sourceMember: "SomeKey",
                destMember: null!,
                context: null!);

            result.Should().Be("LocalizedValue");
        }

        [Fact]
        public void NullableLocalizedPropertyResolver_ReturnsNull_WhenSourceMemberIsNull()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            var resolver = new NullableLocalizedPropertyResolver<object, object>(localizerMock.Object);

            var result = resolver.Resolve(
                source: null!,
                destination: null!,
                sourceMember: null,
                destMember: null!,
                context: null!);

            result.Should().BeNull();
        }

        [Fact]
        public void NullableLocalizedPropertyResolver_ResolvesKey_WhenSourceMemberIsNotNull()
        {
            var localizerMock = new Mock<IHtmlLocalizer<FormsEngineResource>>();
            localizerMock.Setup(l => l["Hello"])
                .Returns(new LocalizedHtmlString("Hello", "HelloLocalized"));

            var resolver = new NullableLocalizedPropertyResolver<object, object>(localizerMock.Object);

            var result = resolver.Resolve(
                source: null!,
                destination: null!,
                sourceMember: "Hello",
                destMember: null!,
                context: null!);

            result.Should().Be("HelloLocalized");
        }
    }
}
