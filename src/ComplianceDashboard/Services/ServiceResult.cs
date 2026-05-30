namespace ComplianceDashboard.Services;

public sealed record ServiceResult<T>(bool Succeeded, T? Value, string? Error, string? Message)
{
    public static ServiceResult<T> Success(T value)
    {
        return new ServiceResult<T>(true, value, null, null);
    }

    public static ServiceResult<T> Failure(string error, string message)
    {
        return new ServiceResult<T>(false, default, error, message);
    }
}