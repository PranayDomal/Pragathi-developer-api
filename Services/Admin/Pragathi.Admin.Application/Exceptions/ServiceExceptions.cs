namespace Pragathi.Admin.Application.Exceptions;

// Something the caller did wrong (bad input) -> we'll turn this into HTTP 400
public class ValidationFailedException : Exception
{
    public ValidationFailedException(string message) : base(message) { }
}

// The thing they asked for doesn't exist -> HTTP 404
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}