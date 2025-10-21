using FluentAssertions;

namespace CO.CDP.Functional.Tests;

public class OptionTests
{
    #region Some Tests

    [Fact]
    public void Some_CreatesOptionWithValue()
    {
        var option = Option<int>.Some(42);

        option.HasValue.Should().BeTrue();
        option.GetValueOrDefault().Should().Be(42);
    }

    [Fact]
    public void Some_WithNullValue_StillHasValue()
    {
        var option = Option<string>.Some(null!);

        option.HasValue.Should().BeTrue();
        option.GetValueOrDefault().Should().BeNull();
    }

    #endregion

    #region None Tests

    [Fact]
    public void None_CreatesOptionWithoutValue()
    {
        var option = Option<int>.None;

        option.HasValue.Should().BeFalse();
    }

    [Fact]
    public void None_GetValueOrDefault_ReturnsDefault()
    {
        var option = Option<int>.None;

        option.GetValueOrDefault().Should().Be(0);
    }

    [Fact]
    public void None_GetValueOrDefault_WithCustomDefault_ReturnsCustom()
    {
        var option = Option<int>.None;

        option.GetValueOrDefault(99).Should().Be(99);
    }

    #endregion

    #region Match Tests (Action)

    [Fact]
    public void Match_WhenSome_ExecutesSomeAction()
    {
        var option = Option<int>.Some(42);
        var executed = false;
        var capturedValue = 0;

        option.Match(
            some: value => { executed = true; capturedValue = value; },
            none: () => { });

        executed.Should().BeTrue();
        capturedValue.Should().Be(42);
    }

    [Fact]
    public void Match_WhenNone_ExecutesNoneAction()
    {
        var option = Option<int>.None;
        var noneExecuted = false;
        var someExecuted = false;

        option.Match(
            some: _ => { someExecuted = true; },
            none: () => { noneExecuted = true; });

        noneExecuted.Should().BeTrue();
        someExecuted.Should().BeFalse();
    }

    #endregion

    #region Match Tests (Func)

    [Fact]
    public void Match_WhenSome_ReturnsSomeResult()
    {
        var option = Option<int>.Some(42);

        var result = option.Match(
            some: value => value * 2,
            none: () => 0);

        result.Should().Be(84);
    }

    [Fact]
    public void Match_WhenNone_ReturnsNoneResult()
    {
        var option = Option<int>.None;

        var result = option.Match(
            some: value => value * 2,
            none: () => -1);

        result.Should().Be(-1);
    }

    [Fact]
    public void Match_WhenSome_ReturnsTransformedType()
    {
        var option = Option<int>.Some(42);

        var result = option.Match(
            some: value => $"Value is {value}",
            none: () => "No value");

        result.Should().Be("Value is 42");
    }

    [Fact]
    public void Match_WhenNone_ReturnsNoneResultOfDifferentType()
    {
        var option = Option<int>.None;

        var result = option.Match(
            some: value => $"Value is {value}",
            none: () => "No value");

        result.Should().Be("No value");
    }

    #endregion

    #region GetValueOrDefault Tests

    [Fact]
    public void GetValueOrDefault_WhenSome_ReturnsValue()
    {
        var option = Option<string>.Some("hello");

        option.GetValueOrDefault("default").Should().Be("hello");
    }

    [Fact]
    public void GetValueOrDefault_WhenNone_ReturnsDefaultValue()
    {
        var option = Option<string>.None;

        option.GetValueOrDefault("default").Should().Be("default");
    }

    [Fact]
    public void GetValueOrDefault_WhenNoneAndNoDefault_ReturnsTypeDefault()
    {
        var option = Option<int>.None;

        option.GetValueOrDefault().Should().Be(0);
    }

    [Fact]
    public void GetValueOrDefault_WhenNoneReferenceType_ReturnsNull()
    {
        var option = Option<string>.None;

        option.GetValueOrDefault().Should().BeNull();
    }

    #endregion

    #region Functional Composition Tests

    [Fact]
    public void Option_CanBeUsedInFunctionalPipeline()
    {
        var result = ParseInt("42")
            .Match(
                some: value => value * 2,
                none: () => 0);

        result.Should().Be(84);
    }

    [Fact]
    public void Option_HandlesInvalidInputGracefully()
    {
        var result = ParseInt("invalid")
            .Match(
                some: value => value * 2,
                none: () => 0);

        result.Should().Be(0);
    }

    private static Option<int> ParseInt(string value) =>
        int.TryParse(value, out var result)
            ? Option<int>.Some(result)
            : Option<int>.None;

    #endregion

    #region Multiple Values Tests

    [Fact]
    public void Option_WorksWithComplexTypes()
    {
        var person = new Person("Alice", 30);
        var option = Option<Person>.Some(person);

        var result = option.Match(
            some: p => p.Name,
            none: () => "Unknown");

        result.Should().Be("Alice");
    }

    [Fact]
    public void Option_OfNullableComplexType()
    {
        var option = Option<Person>.None;

        var result = option.Match(
            some: p => p.Name,
            none: () => "Unknown");

        result.Should().Be("Unknown");
    }

    private record Person(string Name, int Age);

    #endregion
}