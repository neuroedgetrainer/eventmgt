namespace EventMgt.Helpers.Common
{
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? ErrorMessage { get; private set; }
        public int StatusCode { get; private set; }

        public static ServiceResult<T> Success(T data, int statusCode = 200) =>
            new() { IsSuccess = true, Data = data, StatusCode = statusCode };

        public static ServiceResult<T> Failure(string errorMessage, int statusCode = 400) =>
            new() { IsSuccess = false, ErrorMessage = errorMessage, StatusCode = statusCode };
    }
}
