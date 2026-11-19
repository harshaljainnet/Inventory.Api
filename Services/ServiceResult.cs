namespace Inventory.Api.Services
{
    public enum ServiceStatus
    {
        Success,
        NotFound,
        Invalid,
        Conflict
    }

    public record ServiceResult(ServiceStatus Status, string? Error = null, string? Title = null)
    {
        public bool IsSuccess => Status == ServiceStatus.Success;

        public static ServiceResult Ok() => new(ServiceStatus.Success);
        public static ServiceResult NotFound() => new(ServiceStatus.NotFound);
        public static ServiceResult Invalid(string error, string? title = null) => new(ServiceStatus.Invalid, error, title);
        public static ServiceResult Conflict(string error, string? title = null) => new(ServiceStatus.Conflict, error, title);
    }

    public record ServiceResult<T>(ServiceStatus Status, T? Value = default, string? Error = null, string? Title = null)
        : ServiceResult(Status, Error, Title)
    {
        public static ServiceResult<T> Ok(T value) => new(ServiceStatus.Success, value);
        public new static ServiceResult<T> NotFound() => new(ServiceStatus.NotFound);
        public new static ServiceResult<T> Invalid(string error, string? title = null) => new(ServiceStatus.Invalid, default, error, title);
        public new static ServiceResult<T> Conflict(string error, string? title = null) => new(ServiceStatus.Conflict, default, error, title);
    }
}
