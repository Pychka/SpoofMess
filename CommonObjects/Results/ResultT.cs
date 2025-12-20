namespace CommonObjects.Results;

public class Result<T>
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }


    public int StatusCode = 200;

    public T? Body { get; set; }


    public static Result<T> SuccessResult(string message, T body) =>
        new()
        {
            Success = true,
            Message = message,
            Body = body
        };
    public static Result<T> DeletedResult(string message) =>
        new()
        {
            Success = true,
            Message = message
        };

    public static Result<T> NotFoundResult(string message) =>
        new()
        {
            Success = false,
            Error = message,
            StatusCode = 404
        };

    public static Result<T> ErrorResult(string error, int statusCode = 500) =>
        new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
}
