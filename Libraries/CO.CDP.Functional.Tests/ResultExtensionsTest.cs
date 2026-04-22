using FluentAssertions;

namespace CO.CDP.Functional.Tests;

public class ResultExtensionsTests
{
    #region TapResultAsync (on Task<Result>)

    [Fact]
    public async Task TapResultAsync_WhenSuccess_ExecutesAction()
    {
        var captured = 0;
        await Task.FromResult(Result<Exception, int>.Success(7))
            .TapResultAsync(v =>
            {
                captured = v;
                return Task.CompletedTask;
            });
        captured.Should().Be(7);
    }

    #endregion

    #region ToResult

    [Fact]
    public void ToResult_WhenNonNull_ReturnsSuccess()
    {
        string? value = "hello";
        var result = value.ToResult<Exception, string>(() => new InvalidOperationException("null"));
        result.IsSuccess.Should().BeTrue();
        result.GetOrElse("").Should().Be("hello");
    }

    [Fact]
    public void ToResult_WhenNull_ReturnsFailure()
    {
        string? value = null;
        var ex = new InvalidOperationException("was null");
        var result = value.ToResult<Exception, string>(() => ex);
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Ensure

    [Fact]
    public void Ensure_WhenSuccessAndPredicateTrue_ReturnsSuccess()
    {
        var result = Result<Exception, int>.Success(10)
            .Ensure(x => x > 0, () => new InvalidOperationException("not positive"));
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Ensure_WhenSuccessAndPredicateFalse_ReturnsFailure()
    {
        var result = Result<Exception, int>.Success(-1)
            .Ensure(x => x > 0, () => new InvalidOperationException("not positive"));
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Ensure_WhenAlreadyFailure_StaysFailure()
    {
        var original = new InvalidOperationException("original");
        var result = Result<Exception, int>.Failure(original)
            .Ensure(x => x > 0, () => new InvalidOperationException("new"));
        result.IsFailure.Should().BeTrue();
        result.GetOrElse(ex => 0).Should().Be(0);
    }

    #endregion

    #region Tap

    [Fact]
    public void Tap_WhenSuccess_ExecutesAction()
    {
        var captured = 0;
        Result<Exception, int>.Success(5).Tap(v => captured = v);
        captured.Should().Be(5);
    }

    [Fact]
    public void Tap_WhenFailure_DoesNotExecuteAction()
    {
        var executed = false;
        Result<Exception, int>.Failure(new Exception()).Tap(_ => executed = true);
        executed.Should().BeFalse();
    }

    [Fact]
    public void Tap_ReturnsOriginalResult()
    {
        var result = Result<Exception, int>.Success(5);
        var returned = result.Tap(_ => { });
        returned.Should().BeSameAs(result);
    }

    #endregion

    #region TapAsync

    [Fact]
    public async Task TapAsync_WhenSuccess_ExecutesAction()
    {
        var captured = 0;
        await Result<Exception, int>.Success(42).TapAsync(v =>
        {
            captured = v;
            return Task.CompletedTask;
        });
        captured.Should().Be(42);
    }

    [Fact]
    public async Task TapAsync_WhenFailure_DoesNotExecuteAction()
    {
        var executed = false;
        await Result<Exception, int>.Failure(new Exception())
            .TapAsync(_ =>
            {
                executed = true;
                return Task.CompletedTask;
            });
        executed.Should().BeFalse();
    }

    #endregion

    #region Unwrap

    [Fact]
    public void Unwrap_WhenSuccess_ReturnsValue()
    {
        var result = Result<Exception, int>.Success(99);
        result.Unwrap().Should().Be(99);
    }

    [Fact]
    public void Unwrap_WhenFailure_ThrowsException()
    {
        var ex = new InvalidOperationException("oops");
        var result = Result<Exception, int>.Failure(ex);
        var act = () => result.Unwrap();
        act.Should().Throw<InvalidOperationException>().WithMessage("oops");
    }

    #endregion

    #region EnsureAsync (on Task<Result>)

    [Fact]
    public async Task EnsureAsync_WhenSuccessAndPredicateTrue_ReturnsSuccess()
    {
        var result = await Task.FromResult(Result<Exception, int>.Success(10))
            .EnsureAsync(x => x > 0, () => new InvalidOperationException());
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task EnsureAsync_WhenSuccessAndPredicateFalse_ReturnsFailure()
    {
        var result = await Task.FromResult(Result<Exception, int>.Success(-1))
            .EnsureAsync(x => x > 0, () => new InvalidOperationException());
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region UnwrapAsync

    [Fact]
    public async Task UnwrapAsync_WhenSuccess_ReturnsValue()
    {
        var value = await Task.FromResult(Result<Exception, int>.Success(5)).UnwrapAsync();
        value.Should().Be(5);
    }

    [Fact]
    public async Task UnwrapAsync_WhenFailure_ThrowsException()
    {
        var ex = new InvalidOperationException("async oops");
        var act = async () => await Task.FromResult(Result<Exception, int>.Failure(ex)).UnwrapAsync();
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("async oops");
    }

    #endregion
}