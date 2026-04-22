using FluentAssertions;

namespace CO.CDP.Functional.Tests;

public class OptionExtensionsTests
{
    #region Option.From (nullable reference)

    [Fact]
    public void From_NullReference_ReturnsNone()
    {
        string? value = null;
        Option.From(value).HasValue.Should().BeFalse();
    }

    [Fact]
    public void From_NonNullReference_ReturnsSome()
    {
        var opt = Option.From("hello");
        opt.HasValue.Should().BeTrue();
        opt.GetValueOrDefault().Should().Be("hello");
    }

    #endregion

    #region Option.From (nullable value type)

    [Fact]
    public void From_NullableStruct_NoValue_ReturnsNone()
    {
        int? value = null;
        Option.From(value).HasValue.Should().BeFalse();
    }

    [Fact]
    public void From_NullableStruct_WithValue_ReturnsSome()
    {
        int? value = 42;
        var opt = Option.From(value);
        opt.HasValue.Should().BeTrue();
        opt.GetValueOrDefault().Should().Be(42);
    }

    #endregion

    #region Map

    [Fact]
    public void Map_WhenSome_TransformsValue()
    {
        var result = Option<int>.Some(5).Map(x => x * 2);
        result.GetValueOrDefault().Should().Be(10);
    }

    [Fact]
    public void Map_WhenNone_ReturnsNone()
    {
        var result = Option<int>.None.Map(x => x * 2);
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_WhenSome_AppliesFunction()
    {
        var result = Option<int>.Some(5).Bind(x => Option<string>.Some($"val={x}"));
        result.GetValueOrDefault().Should().Be("val=5");
    }

    [Fact]
    public void Bind_WhenNone_ReturnsNone()
    {
        var result = Option<int>.None.Bind(x => Option<string>.Some($"val={x}"));
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Bind_WhenSome_BindReturnsNone_ReturnsNone()
    {
        var result = Option<int>.Some(5).Bind(_ => Option<string>.None);
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region Where

    [Fact]
    public void Where_WhenSome_PredicateTrue_ReturnsSome()
    {
        var result = Option<int>.Some(10).Where(x => x > 5);
        result.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Where_WhenSome_PredicateFalse_ReturnsNone()
    {
        var result = Option<int>.Some(3).Where(x => x > 5);
        result.HasValue.Should().BeFalse();
    }

    [Fact]
    public void Where_WhenNone_ReturnsNone()
    {
        var result = Option<int>.None.Where(x => x > 5);
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_WhenSome_ExecutesAction()
    {
        var captured = 0;
        await Option<int>.Some(42).TapAsync(v =>
        {
            captured = v;
            return Task.CompletedTask;
        });
        captured.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_WhenNone_DoesNotExecuteAction()
    {
        var executed = false;
        await Option<int>.None.TapAsync(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        executed.Should().BeFalse();
    }

    [Fact]
    public async Task TapAsync_OnTaskOption_WhenSome_ExecutesAction()
    {
        var captured = 0;
        await Task.FromResult(Option<int>.Some(7))
            .TapAsync(v =>
            {
                captured = v;
                return Task.CompletedTask;
            });
        captured.Should().Be(7);
    }

    #endregion

    #region BindAsync

    [Fact]
    public async Task BindAsync_WhenSome_AppliesAsyncFunction()
    {
        var result = await Option<int>.Some(5)
            .BindAsync(x => Task.FromResult(Option<string>.Some($"val={x}")));
        result.GetValueOrDefault().Should().Be("val=5");
    }

    [Fact]
    public async Task BindAsync_WhenNone_ReturnsNone()
    {
        var result = await Option<int>.None
            .BindAsync(x => Task.FromResult(Option<string>.Some($"val={x}")));
        result.HasValue.Should().BeFalse();
    }

    #endregion

    #region MapAsync

    [Fact]
    public async Task MapAsync_WhenSome_TransformsValue()
    {
        var result = await Option<int>.Some(5).MapAsync(x => Task.FromResult(x * 2));
        result.GetValueOrDefault().Should().Be(10);
    }

    [Fact]
    public async Task MapAsync_WhenNone_ReturnsNone()
    {
        var result = await Option<int>.None.MapAsync(x => Task.FromResult(x * 2));
        result.HasValue.Should().BeFalse();
    }

    #endregion
}