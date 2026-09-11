namespace ChatRPG.Agents;

internal sealed class Result<T>
{
    public T? Value { get; private set; }
    public List<string> Errors { get; private set; } = [];
    
    private Result() { }

    public bool IsSuccess => Value is not null;
    public bool IsFailure => !IsSuccess;
    
    public static Result<T> Success(T value)
    {
        return new Result<T>
        {
            Value = value
        };
    }

    public static Result<T> Failure(params string[] errors)
    {
        return new Result<T>
        {
            Errors = [.. errors]
        };
    }
    
    public void IfSuccess(Action<T> action)
    {
        if (IsSuccess) action(Value!);
    }

    public void IfFailure(Action action)
    {
        if (IsFailure) action();
    }
}
