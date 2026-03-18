namespace API.Application.Common.Exceptions;

public sealed class UserUniqueConstraintViolationException : Exception
{
    public const string DefaultCode = "USER_ALREADY_EXISTS";

    public UserUniqueConstraintViolationException(string field, string message)
        : base(message)
    {
        Code = DefaultCode;
        Field = field;
    }

    public UserUniqueConstraintViolationException(string field)
        : this(field, field switch
        {
            "phone" => "Phone already exists",
            "username" => "Username already exists",
            _ => "User already exists"
        })
    {
    }

    public string Code { get; }

    public string Field { get; }
}

