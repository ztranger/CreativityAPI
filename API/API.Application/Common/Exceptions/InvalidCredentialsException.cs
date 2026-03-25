namespace API.Application.Common.Exceptions;

public sealed class InvalidCredentialsException : Exception
{
    public const string DefaultCode = "INVALID_CREDENTIALS";

    public InvalidCredentialsException(string message = "Invalid phone or password.")
        : base(message)
    {
    }

    public string Code => DefaultCode;
}
