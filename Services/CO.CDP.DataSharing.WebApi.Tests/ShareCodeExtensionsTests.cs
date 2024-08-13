using CO.CDP.DataSharing.WebApi.Extensions;
using FluentAssertions;
using NanoidDotNet;

namespace CO.CDP.DataSharing.WebApi.Tests;

public class ShareCodeExtensionsTests
{
    [Fact]
    public void GenerateShareCode_IsNotNullEmptyOrWhiteSpace()
    {
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        shareCode.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateValidShareCode_HasValidLength()
    {
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        shareCode.Should().HaveLength(8);
    }

    [Fact]
    public void GenerateValidShareCode_ContainsValidCharsOnly()
    {
        var shareCode = ShareCodeExtensions.GenerateShareCode();

        shareCode.All(x => Nanoid.Alphabets.NoLookAlikesSafe.Contains(x));
    }
}