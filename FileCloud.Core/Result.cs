using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core
{
    public abstract class Result
    {
        [MemberNotNullWhen(false, nameof(Error))]
        public abstract bool IsSuccess { get; }
        public abstract string? Error { get; }

        // Для необобщенного доступа к значению (если нужно)
        public abstract object? GetValue();

        public static Result Success() => new Result<object>(true, null, true);
        public static Result<T> Success<T>(T value) => Result<T>.Success(value);
        public static Result Fail(string error) => new Result<object>(default, error, false);
        public static Result<T> Fail<T>(string error) => Result<T>.Fail(error);
    }

    public class Result<T> : Result
    {
        [MemberNotNullWhen(true, nameof(Value))]
        [MemberNotNullWhen(false, nameof(Error))]
        public override bool IsSuccess { get; }
        public override string? Error { get; }
        public T? Value { get; }

        internal Result(T? value, string? error, bool isSuccess)
        {
            Value = value;
            Error = error;
            IsSuccess = isSuccess;
        }

        public static Result<T> Success(T value) => new(value, null, true);
        public static Result<T> Fail(string error) => new(default, error, false);

        public override object? GetValue() => Value;
    }
}
