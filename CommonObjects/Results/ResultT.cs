namespace CommonObjects.Results;

public class Result<T>
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }


    public int StatusCode = 200;

    public T? Body { get; set; }


    public static Result<T> SuccessResult(T body, string? message = "OK", int statusCode = 200) =>
        new()
        {
            Success = true,
            Message = message,
            StatusCode = statusCode,
            Body = body
        };

    public static Result<T> OkResult(T body) =>
        SuccessResult(body);

    public static Result<T> NotFoundResult(string error) =>
        ErrorResult(error, 404);

    public static Result<T> BadRequest(string error) =>
        ErrorResult(error, 400);

    public static Result<T> ErrorResult(string error, int statusCode = 500) =>
        new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };

    public static Result<T> From(Result result) =>
        new()
        {
            Success = result.Success,
            Error = result.Error,
            StatusCode = result.StatusCode,
            Message = result.Message,
        };
}
