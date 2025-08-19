using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core
{
    public class Result<T>
    {
        public bool IsSuccess { get; }
        public string? Error { get; }
        public T? Value { get; }

        private Result(T? value, string? error, bool isSuccess)
        {
            Value = value;
            Error = error;
            IsSuccess = isSuccess;
        }

        public static Result<T> Success(T value) => new(value, null, true);
        public static Result<T> Fail(string error) => new(default, error, false);
    }
}
