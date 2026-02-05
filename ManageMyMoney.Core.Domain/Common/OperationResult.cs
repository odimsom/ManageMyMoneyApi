namespace ManageMyMoney.Core.Domain.Common;

public class OperationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected OperationResult(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Success result cannot have error");

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
            throw new InvalidOperationException("Failure result must have error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static OperationResult Success() => new(true, string.Empty);

    public static OperationResult Failure(string error) => new(false, error);

    public static OperationResult<T> Success<T>(T value) => new(value, true, string.Empty);

    public static OperationResult<T> Failure<T>(string error) => new(default, false, error);
}

public class OperationResult<T> : OperationResult
{
    public T? Value { get; }

    protected internal OperationResult(T? value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }
}
