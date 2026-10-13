namespace CalibrationManagement.Application.Common;

public abstract class AppException : Exception
{
    public int StatusCode { get; }
    protected AppException(string message, int statusCode) : base(message) => StatusCode = statusCode;
}

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message, 404) { }
}

public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message, 409) { }
}

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message, 422) { }
}

public sealed class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message, 401) { }
}

public sealed class AppValidationException : AppException
{
    public IDictionary<string, string[]> Errors { get; }

    public AppValidationException(string message) : base(message, 400)
        => Errors = new Dictionary<string, string[]> { ["request"] = new[] { message } };

    public AppValidationException(string field, string message) : base(message, 400)
        => Errors = new Dictionary<string, string[]> { [field] = new[] { message } };

    public AppValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", 400) => Errors = errors;
}
