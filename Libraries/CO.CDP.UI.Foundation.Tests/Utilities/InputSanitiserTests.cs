using CO.CDP.UI.Foundation.Utilities;
using Xunit;

namespace CO.CDP.UI.Foundation.Tests.Utilities;

public class InputSanitiserTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "")]
    [InlineData("abc", "abc")]
    [InlineData("  abc  ", "abc")]
    [InlineData("abc   def", "abc def")]
    [InlineData("abc\ndef", "abc def")]
    [InlineData("abc\r\ndef", "abc def")]
    [InlineData("abc\tdef", "abc def")]
    [InlineData("abc<def>ghi", "abcdefghi")]
    [InlineData("<script>alert('x')</script>", "scriptalert('x')/script")]
    [InlineData("a < b > c", "a b c")]
    [InlineData("a\u0000b\u0001c", "a b c")]
    [InlineData("  <b>hello</b>  ", "bhello/b")]
    [InlineData("radio television", "radio television")]
    [InlineData("administration + defence", "administration + defence")]
    [InlineData("\"market research\"", "\"market research\"")]

    public void SanitiseSingleLineTextInput_ReturnsExpectedResult(string? input, string? expected)
    {
        var result = InputSanitiser.SanitiseSingleLineTextInput(input);
        Assert.Equal(expected, result);
    }
}