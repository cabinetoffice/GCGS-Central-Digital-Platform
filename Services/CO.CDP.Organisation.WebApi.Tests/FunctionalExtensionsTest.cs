using FluentAssertions;

namespace CO.CDP.Organisation.WebApi.Tests;
public class FunctionalExtensionsTest
{
    [Fact]
    public async Task ItContinuesExecutionOnTaskCompletion()
    {
        var result = await Task.FromResult(42)
            .AndThen(i => i + 2);

        result.Should().Be(44);
    }
}
