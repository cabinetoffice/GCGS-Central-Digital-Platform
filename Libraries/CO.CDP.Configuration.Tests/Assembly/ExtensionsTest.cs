using CO.CDP.Configuration.Assembly;
using FluentAssertions;
using static System.Reflection.Assembly;

namespace CO.CDP.Configuration.Tests.Assembly;

public class ExtensionsTest
{
    [Fact]
    public void ItChecksIfNameMatchesTheExpectedOne()
    {
        GetEntryAssembly().IsRunAs("test").Should().Be(true);
        GetEntryAssembly().IsRunAs("Boo Boo").Should().Be(false);
    }
}