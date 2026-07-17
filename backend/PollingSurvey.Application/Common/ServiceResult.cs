namespace PollingSurvey.Application.Common;

public enum ServiceResultStatus
{
    Success,
    NotFound,
    Conflict,
    Forbidden,
    ValidationError,
    Unauthorized
}

public class ServiceResult<T>
{
    public ServiceResultStatus Status { get; }
    public T? Data { get; }
    public string? Message { get; }

    private ServiceResult(ServiceResultStatus status, T? data, string? message)
    {
        Status = status;
        Data = data;
        Message = message;
    }

    public static ServiceResult<T> Success(T data) => new(ServiceResultStatus.Success, data, null);
    public static ServiceResult<T> NotFound(string message) => new(ServiceResultStatus.NotFound, default, message);
    public static ServiceResult<T> Conflict(string message) => new(ServiceResultStatus.Conflict, default, message);
    public static ServiceResult<T> Forbidden(string message) => new(ServiceResultStatus.Forbidden, default, message);
    public static ServiceResult<T> ValidationError(string message) => new(ServiceResultStatus.ValidationError, default, message);
    public static ServiceResult<T> Unauthorized(string message) => new(ServiceResultStatus.Unauthorized, default, message);
}
