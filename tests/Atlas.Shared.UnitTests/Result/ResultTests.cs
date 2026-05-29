using Atlas.Shared.Result;
using FluentAssertions;
using R = Atlas.Shared.Result.Result;

namespace Atlas.Shared.UnitTests.Result;

public sealed class ResultTests
{
    private sealed record TestError(string Code, string Message) : Error(Code, Message);

    private static readonly TestError SampleError = new("test.boom", "Boom.");

    [Fact]
    public void Ok_creates_a_success_result()
    {
        R result = R.Ok();

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Fail_creates_a_failure_result_with_error()
    {
        R result = R.Fail(SampleError);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Implicit_conversion_from_error_yields_failure()
    {
        R result = SampleError;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Bind_executes_next_only_on_success()
    {
        bool wasCalled = false;
        R.Ok().Bind(() =>
        {
            wasCalled = true;
            return R.Ok();
        });
        wasCalled.Should().BeTrue();

        wasCalled = false;
        R.Fail(SampleError).Bind(() =>
        {
            wasCalled = true;
            return R.Ok();
        });
        wasCalled.Should().BeFalse();
    }

    [Fact]
    public void Bind_generic_propagates_error_to_typed_result()
    {
        Result<int> result = R.Fail(SampleError).Bind(() => Result<int>.Ok(42));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Match_dispatches_to_branch()
    {
        string success = R.Ok().Match(() => "ok", _ => "ko");
        string failure = R.Fail(SampleError).Match(() => "ok", e => $"ko:{e.Code}");

        success.Should().Be("ok");
        failure.Should().Be("ko:test.boom");
    }

    [Fact]
    public void Tap_runs_side_effect_only_on_success()
    {
        int counter = 0;
        R.Ok().Tap(() => counter++);
        R.Fail(SampleError).Tap(() => counter++);

        counter.Should().Be(1);
    }
}
