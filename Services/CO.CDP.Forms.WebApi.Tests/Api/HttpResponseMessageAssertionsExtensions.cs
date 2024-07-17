using System.Net.Http.Json;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace CO.CDP.Forms.WebApi.Tests.Api;

internal static class HttpResponseMessageAssertionsExtensions
{
    internal static async Task HaveContent<T>(this HttpResponseMessageAssertions assertions, T content)
    {
        using (new AssertionScope("Content"))
        {
                var actualContent = await assertions.Subject.Content.ReadFromJsonAsync<T>();
                actualContent.Should().BeEquivalentTo(content);
        }
    }
}