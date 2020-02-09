using System;

namespace CSharpMath.Structures {
  public readonly struct Result {
    //For Result<string> where both implicit conversions fight over each other,
    //use Err(string) there instead
    public readonly struct ImplicitError {
      public readonly string error;
      public ImplicitError(string error) =>
        this.error = error ??
          throw new ArgumentNullException(nameof(error), "There is no error.");
    }
    public static Result Ok() => new Result();
    public static Result<T> Ok<T>(T value) => new Result<T>(value);
    public static SpanResult<T> Ok<T>(ReadOnlySpan<T> value) => new SpanResult<T>(value);
    public static ImplicitError Err(string error) => new ImplicitError(error);
    public Result(string error) =>
      Error = error ?? throw new ArgumentNullException(nameof(error), "There is no error.");
    public string? Error { get; }
    public void Match(Action<string> errorAction) { if (Error != null) errorAction(Error); }
    public static implicit operator Result(string error) => new Result(error);
    public static implicit operator Result(ImplicitError error) => new Result(error.error);
  }
  public readonly struct Result<T> {
    public Result(T value) => (_value, Error) = (value, null);
    public Result(string error) =>
      (_value, Error) = (default!, error ??
        throw new ArgumentNullException(nameof(error), "There is no error."));
    internal readonly T _value;
    public string? Error { get; }
    public void Deconstruct(out T value, out string? error) =>
      (value, error) = (_value, Error);
    public void Match(Action<T> successAction, Action<string> errorAction)
      { if (Error != null) errorAction(Error); else successAction(_value); }
    public TResult Match<TResult>(Func<T, TResult> successAction,
      Func<string, TResult> errorAction) =>
      Error != null ? errorAction(Error) : successAction(_value);
    public static implicit operator Result<T>(T value) =>
      new Result<T>(value);
    public static implicit operator Result<T>(string error) =>
      new Result<T>(error);
    public static implicit operator Result<T>(Result.ImplicitError error) =>
      new Result<T>(error.error);
    public Result Bind(Action<T> method) {
      if (Error is string error) return error;
      else method(_value);
      return Result.Ok();
    }
    public Result Bind(Func<T, Result> method) {
      if (Error is string error) return error;
      else return method(_value);
    }
    public Result<TResult> Bind<TResult>(Func<T, TResult> method) {
      if (Error is string error) return error;
      else return method(_value);
    }
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> method) {
      if (Error is string error) return error;
      else return method(_value);
    }
    public Result Bind<TOther>(Result<TOther> other, Action<T, TOther> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else method(_value, other._value);
      return Result.Ok();
    }
    public Result Bind<TOther>(Result<TOther> other, Func<T, TOther, Result> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(_value, other._value);
    }
    public Result<TResult> Bind<TOther, TResult>
      (Result<TOther> other, Func<T, TOther, TResult> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(_value, other._value);
    }
    public Result<TResult> Bind<TOther, TResult>
      (Result<TOther> other, Func<T, TOther, Result<TResult>> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(_value, other._value);
    }
  }
  public readonly ref struct SpanResult<T> {
    public SpanResult(ReadOnlySpan<T> value) {
      _value = value;
      Error = null;
    }
    public SpanResult(string error) {
      _value = default;
      Error = error ?? throw new ArgumentNullException(nameof(error), "There is no error.");
    }
    private readonly ReadOnlySpan<T> _value;
    public string? Error { get; }
    public void Deconstruct(out ReadOnlySpan<T> value, out string? error) {
      value = _value;
      error = Error;
    }
    public void Match(Action successAction, System.Action<string> errorAction)
      { if (Error != null) errorAction(Error); else successAction(_value); }
    public TResult Match<TResult>
      (Func<TResult> successAction, System.Func<string, TResult> errorAction) =>
      Error != null ? errorAction(Error) : successAction(_value);
    public static implicit operator SpanResult<T>(ReadOnlySpan<T> value) =>
      new SpanResult<T>(value);
    public static implicit operator SpanResult<T>(string error) =>
      new SpanResult<T>(error);
    public static implicit operator SpanResult<T>(Result.ImplicitError error) =>
      new SpanResult<T>(error.error);
    public delegate void Action(ReadOnlySpan<T> result);
    public delegate void Action<TOther>(ReadOnlySpan<T> thisResult, TOther otherResult);
    public delegate TResult Func<TResult>(ReadOnlySpan<T> result);
    public delegate TResult Func<TOther, TResult>(ReadOnlySpan<T> thisResult, TOther otherResult);
    public Result Bind(Action method) {
      if (Error is string error) return error;
      else method(_value);
      return Result.Ok();
    }
    public Result Bind(Func<Result> method) {
      if (Error is string error) return error;
      else return method(_value);
    }
    public Result<TResult> Bind<TResult>(Func<TResult> method) {
      if (Error is string error) return error;
      else return method(_value);
    }
    public Result<TResult> Bind<TResult>(Func<Result<TResult>> method) {
      if (Error is string error) return error;
      else return method(_value);
    }
    public Result Bind<TOther>(Result<TOther> other, Action<TOther> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else method(_value, other._value);
      return Result.Ok();
    }
    public Result Bind<TOther>(Result<TOther> other, Func<TOther, Result> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(_value, other._value);
    }
    public Result<TResult> Bind<TOther, TResult>
      (Result<TOther> other, Func<TOther, TResult> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(_value, other._value);
    }
    public Result<TResult> Bind<TOther, TResult>
      (Result<TOther> other, Func<TOther, Result<TResult>> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(_value, other._value);
    }
  }
}