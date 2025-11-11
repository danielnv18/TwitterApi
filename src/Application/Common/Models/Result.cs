namespace TwitterCloneApi.Application.Common.Models;

public class Result<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public IDictionary<string, string[]>? ValidationErrors { get; init; }

    private Result() { }

    public static Result<T> Success(T data)
    {
        return new Result<T>
        {
            IsSuccess = true,
            Data = data
        };
    }

    public static Result<T> Failure(string errorMessage)
    {
        return new Result<T>
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }

    public static Result<T> ValidationFailure(IDictionary<string, string[]> errors)
    {
        return new Result<T>
        {
            IsSuccess = false,
            ValidationErrors = errors,
            ErrorMessage = "One or more validation errors occurred."
        };
    }
}
