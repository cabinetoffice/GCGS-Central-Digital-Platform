using FluentAssertions;

namespace CO.CDP.Functional.Tests;

public class EitherTest
{
    [Fact]
    public void Left_CreatesLeftCase()
    {
        var either = Either<string, int>.Left("error");

        either.IsLeft().Should().BeTrue();
        either.IsRight().Should().BeFalse();
    }

    [Fact]
    public void Right_CreatesRightCase()
    {
        var either = Either<string, int>.Right(42);

        either.IsRight().Should().BeTrue();
        either.IsLeft().Should().BeFalse();
    }

    [Fact]
    public void Match_WithLeftCase_CallsOnLeft()
    {
        var either = Either<string, int>.Left("error");

        var result = either.Match(
            left => $"Error: {left}",
            right => $"Success: {right}"
        );

        result.Should().Be("Error: error");
    }

    [Fact]
    public void Match_WithRightCase_CallsOnRight()
    {
        var either = Either<string, int>.Right(42);

        var result = either.Match(
            left => $"Error: {left}",
            right => $"Success: {right}"
        );

        result.Should().Be("Success: 42");
    }

    [Fact]
    public async Task MatchAsync_WithLeftCase_CallsOnLeft()
    {
        var either = Either<string, int>.Left("error");

        var result = await either.MatchAsync(
            async left => await Task.FromResult($"Error: {left}"),
            async right => await Task.FromResult($"Success: {right}")
        );

        result.Should().Be("Error: error");
    }

    [Fact]
    public async Task MatchAsync_WithRightCase_CallsOnRight()
    {
        var either = Either<string, int>.Right(42);

        var result = await either.MatchAsync(
            async left => await Task.FromResult($"Error: {left}"),
            async right => await Task.FromResult($"Success: {right}")
        );

        result.Should().Be("Success: 42");
    }

    [Fact]
    public void GetOrElse_WithRightCase_ReturnsRightValue()
    {
        var either = Either<string, int>.Right(42);

        var result = either.GetOrElse(0);

        result.Should().Be(42);
    }

    [Fact]
    public void GetOrElse_WithLeftCase_ReturnsDefaultValue()
    {
        var either = Either<string, int>.Left("error");

        var result = either.GetOrElse(0);

        result.Should().Be(0);
    }

    [Fact]
    public void GetOrElse_WithFunction_WithLeftCase_CallsDefaultProvider()
    {
        var either = Either<string, int>.Left("error");

        var result = either.GetOrElse(err => err.Length);

        result.Should().Be(5);
    }

    [Fact]
    public void GetOrElse_WithFunction_WithRightCase_ReturnsRightValue()
    {
        var either = Either<string, int>.Right(42);

        var result = either.GetOrElse(err => err.Length);

        result.Should().Be(42);
    }

    [Fact]
    public void Map_WithRightCase_TransformsRightValue()
    {
        var either = Either<string, int>.Right(42);

        var result = either.Map(x => x * 2);

        result.GetOrElse(0).Should().Be(84);
    }

    [Fact]
    public void Map_WithLeftCase_PreservesLeftValue()
    {
        var either = Either<string, int>.Left("error");

        var result = either.Map(x => x * 2);

        result.Match(
            left => left.Should().Be("error"),
            right => Assert.Fail("Should not be right")
        );
    }

    [Fact]
    public async Task MapAsync_WithRightCase_TransformsRightValue()
    {
        var either = Either<string, int>.Right(42);

        var result = await either.MapAsync(async x => await Task.FromResult(x * 2));

        result.GetOrElse(0).Should().Be(84);
    }

    [Fact]
    public async Task MapAsync_WithLeftCase_PreservesLeftValue()
    {
        var either = Either<string, int>.Left("error");

        var result = await either.MapAsync(async x => await Task.FromResult(x * 2));

        result.Match(
            left => left.Should().Be("error"),
            right => Assert.Fail("Should not be right")
        );
    }

    [Fact]
    public void Bind_WithRightCase_AppliesFunction()
    {
        var either = Either<string, int>.Right(42);

        var result = either.Bind(x =>
            x > 0
                ? Either<string, string>.Right($"Positive: {x}")
                : Either<string, string>.Left("Not positive")
        );

        result.GetOrElse("").Should().Be("Positive: 42");
    }

    [Fact]
    public void Bind_WithLeftCase_PreservesLeftValue()
    {
        var either = Either<string, int>.Left("error");

        var result = either.Bind(x =>
            x > 0
                ? Either<string, string>.Right($"Positive: {x}")
                : Either<string, string>.Left("Not positive")
        );

        result.Match(
            left => left.Should().Be("error"),
            right => Assert.Fail("Should not be right")
        );
    }

    [Fact]
    public async Task BindAsync_WithRightCase_AppliesFunction()
    {
        var either = Either<string, int>.Right(42);

        var result = await either.BindAsync(async x =>
            await Task.FromResult(
                x > 0
                    ? Either<string, string>.Right($"Positive: {x}")
                    : Either<string, string>.Left("Not positive")
            )
        );

        result.GetOrElse("").Should().Be("Positive: 42");
    }

    [Fact]
    public void OnLeft_WithLeftCase_ExecutesAction()
    {
        var either = Either<string, int>.Left("error");
        var executed = false;

        either.OnLeft(_ => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public void OnLeft_WithRightCase_DoesNotExecuteAction()
    {
        var either = Either<string, int>.Right(42);
        var executed = false;

        either.OnLeft(_ => executed = true);

        executed.Should().BeFalse();
    }

    [Fact]
    public void OnRight_WithRightCase_ExecutesAction()
    {
        var either = Either<string, int>.Right(42);
        var executed = false;

        either.OnRight(_ => executed = true);

        executed.Should().BeTrue();
    }

    [Fact]
    public void OnRight_WithLeftCase_DoesNotExecuteAction()
    {
        var either = Either<string, int>.Left("error");
        var executed = false;

        either.OnRight(_ => executed = true);

        executed.Should().BeFalse();
    }
}
