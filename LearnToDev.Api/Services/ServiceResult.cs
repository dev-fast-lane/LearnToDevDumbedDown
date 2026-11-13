namespace LearnToDev.Api.Services;

//The possible outcomes of a service call. The controller turns them into status codes
public enum ServiceStatus
{
    Success,  //200
    NotFound, //404
    Invalid,  //400: the request breaks a rule
    Conflict  //409: the request is fine, but not allowed right now
}

//A receipt from the service: what happened, the value if it went well, and why if it didn't.
//T is the type of the value, e.g. ExamResultDto
public record ServiceResult<T>(ServiceStatus Status, T? Value = default, string? Message = null)
{
    public static ServiceResult<T> Ok(T value) => new(ServiceStatus.Success, value);

    public static ServiceResult<T> NotFound(string message) => new(ServiceStatus.NotFound, default, message);

    public static ServiceResult<T> Invalid(string message) => new(ServiceStatus.Invalid, default, message);

    public static ServiceResult<T> Conflict(string message) => new(ServiceStatus.Conflict, default, message);
}
