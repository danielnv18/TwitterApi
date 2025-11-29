namespace TwitterCloneApi.Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string name, object key)
        : base($"{name} with id '{key}' already exists.")
    {
    }
}
