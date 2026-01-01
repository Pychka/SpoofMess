namespace CommonObjects.Results;

public class Result
{
    public bool Success { get; set; }

    public string? Message { get; set; }

    public string? Error { get; set; }

    public int StatusCode = 200;

    public static Result SuccessResult(string message = "Ok", int statusCode = 200) =>
        new()
        {
            Success = true,
            Message = message,
            StatusCode = statusCode
        };

    public static Result OkResult(string message = "Ok") =>
        SuccessResult(message);

    public static Result DeletedResult(string message = "Deleted") =>
        SuccessResult(message, 204);

    public static Result NotFoundResult(string error) =>
        ErrorResult(error, 404);

    public static Result BadRequest(string error) =>
        ErrorResult(error, 400);

    public static Result ErrorResult(string error, int statusCode = 500) =>
        new()
        {
            Success = false,
            Error = error,
            StatusCode = statusCode
        };
}
