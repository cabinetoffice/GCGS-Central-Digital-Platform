using CO.CDP.DataSharing.WebApi.Extensions;
using FluentAssertions;
using NanoidDotNet;

namespace DataSharing.Tests;

public class ShareCodeTests
{
    [Fact]
    public void GenerateShareCode_IsNotNullEmptyOrWhiteSpace()
    {
        var shareCode = ShareCodeExtensions.GenerateShareCodeString();

        shareCode.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateValidShareCode_HasValidLength()
    {
        var shareCode = ShareCodeExtensions.GenerateShareCodeString();

        shareCode.Should().HaveLength(8);
    }

    [Fact]
    public void GenerateValidShareCode_ContainsValidCharsOnly()
    {
        var shareCode = ShareCodeExtensions.GenerateShareCodeString();

        shareCode.All(x => Nanoid.Alphabets.NoLookAlikesSafe.Contains(x));
    }
}