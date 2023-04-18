namespace Dyconits.Exceptions;

public class DyconitsException : Exception
{
    public DyconitsException(string message) : base(message)
    {
    }

    public DyconitsException(string message, Exception exception) : base(message, exception)
    {
    }
}
