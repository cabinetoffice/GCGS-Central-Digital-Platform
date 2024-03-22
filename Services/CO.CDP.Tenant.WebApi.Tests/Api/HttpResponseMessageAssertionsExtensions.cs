using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace CO.CDP.Tenant.WebApi.Tests.Api;

internal static class HttpResponseMessageAssertionsExtensions
{
    internal static void MatchLocation(this HttpResponseMessageAssertions assertions, string regex)
    {
        using (new AssertionScope("Location header"))
        {
            assertions.Subject.Headers.Location.Should().NotBeNull();
            assertions.Subject.Headers.Location?.OriginalString.Should().MatchRegex(regex);
        }
    }

    internal static async Task HaveContent<T>(this HttpResponseMessageAssertions assertions, T content)
    {
        using (new AssertionScope("Content"))
        {
            (await assertions.Subject.Content.ReadFromJsonAsync<T>()).Should().Be(content);
        }
    }
}