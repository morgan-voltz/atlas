using Atlas.Shared.Result;
using FluentAssertions;

namespace Atlas.Shared.UnitTests.Result;

public sealed class ResultOfTTests
{
    private sealed record TestError(string Code, string Message) : Error(Code, Message);

    private static readonly TestError SampleError = new("test.boom", "Boom.");

    [Fact]
    public void Ok_creates_a_success_result_with_value()
    {
        Result<int> result = Result<int>.Ok(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Value_on_failure_throws_with_diagnostic_message()
    {
        Result<int> result = Result<int>.Fail(SampleError);

        FluentActions.Invoking(() => result.Value)
            .Should().Throw<InvalidOperationException>()
            .WithMessage("*test.boom*");
    }

    [Fact]
    public void TryGetValue_returns_false_on_failure_without_throwing()
    {
        Result<int> result = Result<int>.Fail(SampleError);

        bool got = result.TryGetValue(out int value);

        got.Should().BeFalse();
        value.Should().Be(0);
    }

    [Fact]
    public void TryGetValue_returns_true_on_success()
    {
        Result<string> result = Result<string>.Ok("ok");

        bool got = result.TryGetValue(out string? value);

        got.Should().BeTrue();
        value.Should().Be("ok");
    }

    [Fact]
    public void Implicit_conversion_from_error_yields_failure()
    {
        Result<int> result = SampleError;

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Map_transforms_value_only_on_success()
    {
        Result<int> length = Result<string>.Ok("hello").Map(s => s.Length);
        length.Value.Should().Be(5);

        Result<int> failed = Result<string>.Fail(SampleError).Map(s => s.Length);
        failed.IsFailure.Should().BeTrue();
        failed.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Bind_chains_typed_results_short_circuiting_on_failure()
    {
        Result<int> chained = Result<string>.Ok("42").Bind(s => Result<int>.Ok(int.Parse(s, System.Globalization.CultureInfo.InvariantCulture)));
        chained.Value.Should().Be(42);

        Result<int> failed = Result<string>.Fail(SampleError).Bind(_ => Result<int>.Ok(99));
        failed.IsFailure.Should().BeTrue();
        failed.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Bind_to_unit_short_circuits_on_failure()
    {
        bool called = false;
        global::Atlas.Shared.Result.Result resultFromOk = Result<int>.Ok(1).Bind(_ =>
        {
            called = true;
            return global::Atlas.Shared.Result.Result.Ok();
        });
        called.Should().BeTrue();
        resultFromOk.IsSuccess.Should().BeTrue();

        called = false;
        global::Atlas.Shared.Result.Result resultFromFail = Result<int>.Fail(SampleError).Bind(_ =>
        {
            called = true;
            return global::Atlas.Shared.Result.Result.Ok();
        });
        called.Should().BeFalse();
        resultFromFail.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Match_dispatches_to_correct_branch()
    {
        string okBranch = Result<int>.Ok(7).Match(v => $"ok:{v}", e => $"ko:{e.Code}");
        string koBranch = Result<int>.Fail(SampleError).Match(v => $"ok:{v}", e => $"ko:{e.Code}");

        okBranch.Should().Be("ok:7");
        koBranch.Should().Be("ko:test.boom");
    }

    [Fact]
    public void Tap_runs_side_effect_only_on_success()
    {
        int captured = 0;
        Result<int>.Ok(3).Tap(v => captured = v);
        captured.Should().Be(3);

        Result<int>.Fail(SampleError).Tap(_ => captured = 999);
        captured.Should().Be(3);
    }

    [Fact]
    public void Ensure_fails_when_predicate_fails_otherwise_passes_through()
    {
        Error tooSmall = new TestError("test.too_small", "Value too small.");

        Result<int> ok = Result<int>.Ok(10).Ensure(v => v >= 5, tooSmall);
        ok.IsSuccess.Should().BeTrue();
        ok.Value.Should().Be(10);

        Result<int> ko = Result<int>.Ok(2).Ensure(v => v >= 5, tooSmall);
        ko.IsFailure.Should().BeTrue();
        ko.Error.Should().Be(tooSmall);

        Result<int> alreadyFailed = Result<int>.Fail(SampleError).Ensure(v => v >= 5, tooSmall);
        alreadyFailed.Error.Should().Be(SampleError);
    }

    [Fact]
    public void Ok_rejects_null_value()
    {
        FluentActions.Invoking(() => Result<string>.Ok(null!))
            .Should().Throw<ArgumentNullException>();
    }
}
