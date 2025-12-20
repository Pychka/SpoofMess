namespace CommonObjects.Results;

public class Result
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public int StatusCode = 200;

    public static Result SuccessResult(string message = "Ok") =>
        new()
        {
            Success = true,
            Message = message
        };

    public static Result DeletedResult(string message) =>
        new()
        {
            Success = true,
            Message = message
        };

    public static Result NotFoundResult(string message) =>
        new()
        {
            Success = false,
            Error = message,
            StatusCode = 404
        };

    public static Result ErrorResult(string error, int statusCode = 500) =>
        new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
}
