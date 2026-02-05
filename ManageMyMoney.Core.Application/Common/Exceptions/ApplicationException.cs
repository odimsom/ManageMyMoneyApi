namespace ManageMyMoney.Core.Application.Common.Exceptions;

public abstract class ApplicationException : Exception
{
    public string Code { get; }

    protected ApplicationException(string code, string message) : base(message)
    {
        Code = code;
    }
}

public class ValidationException : ApplicationException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string error)
        : base("VALIDATION_ERROR", error)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { error } }
        };
    }
}

public class NotFoundException : ApplicationException
{
    public NotFoundException(string entityName, object key)
        : base("NOT_FOUND", $"Entity '{entityName}' with key '{key}' was not found.")
    {
    }
}

public class UnauthorizedException : ApplicationException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base("UNAUTHORIZED", message)
    {
    }
}

public class ForbiddenException : ApplicationException
{
    public ForbiddenException(string message = "Access forbidden.")
        : base("FORBIDDEN", message)
    {
    }
}

public class ConflictException : ApplicationException
{
    public ConflictException(string message)
        : base("CONFLICT", message)
    {
    }
}
