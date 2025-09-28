namespace FileCloud.Contracts
{
    public class ApiResult<T>
    {
        public T? Response { get; set; }
        public string? Error { get; set; }

        public static ApiResult<T> Success(T response)
        {
            return new ApiResult<T> { Response = response };
        }

        public static ApiResult<T> Fail(string error)
        {
            return new ApiResult<T> { Error = error };
        }
    }
}
