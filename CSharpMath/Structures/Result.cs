using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
  public readonly struct Result {
    //For Result<string> where both implicit conversions fight over each other, use Err(string) there instead
    public readonly struct ImplicitError {
      public readonly string error;
      public ImplicitError(string error) =>
        this.error = error ?? throw new ArgumentNullException(nameof(error), "There is no error.");
    }
    public static Result Ok() => new Result();
    public static Result<T> Ok<T>(T value) => new Result<T>(value);
    public static ImplicitError Err(string error) => new ImplicitError(error);

    public Result(string error) =>
      Error = error ?? throw new ArgumentNullException(nameof(error), "There is no error.");

    public string Error { get; }

    public static implicit operator Result(string error) => new Result(error);
    public static implicit operator Result(ImplicitError error) => new Result(error.error);
  }
  public readonly struct Result<T> {
    public Result(T value) =>
      (_value, Error) = (value, null);

    public Result(string error) =>
      (_value, Error) = (default, error ??
        throw new ArgumentNullException(nameof(error), "There is no error."));

    private readonly T _value;
    public T Value => Error != null ? throw new InvalidOperationException(Error) : _value;
    public string Error { get; }

    public static implicit operator Result<T>(T value) => new Result<T>(value);
    public static implicit operator Result<T>(string error) => new Result<T>(error);
    public static implicit operator Result<T>(Result.ImplicitError error) => new Result<T>(error.error);
    
    public Result Bind(Action<T> method) {
      if (Error is string error) return error;
      else method(Value);
      return Result.Ok();
    }
    public Result Bind(Func<T, Result> method) {
      if (Error is string error) return error;
      else return method(Value);
    }
    public Result<TResult> Bind<TResult>(Func<T, TResult> method) {
      if (Error is string error) return error;
      else return method(Value);
    }
    public Result<TResult> Bind<TResult>(Func<T, Result<TResult>> method) {
      if (Error is string error) return error;
      else return method(Value);
    }
    public Result Bind<TOther>(Result<TOther> other, Action<T, TOther> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else method(Value, other.Value);
      return Result.Ok();
    }
    public Result Bind<TOther>(Result<TOther> other, Func<T, TOther, Result> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(Value, other.Value);
    }
    public Result<TResult> Bind<TOther, TResult>(Result<TOther> other, Func<T, TOther, TResult> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(Value, other.Value);
    }
    public Result<TResult> Bind<TOther, TResult>(Result<TOther> other, Func<T, TOther, Result<TResult>> method) {
      if (Error is string error) return error;
      else if (other.Error is string otherError) return otherError;
      else return method(Value, other.Value);
    }
  }
}
