namespace ManageMyMoney.Core.Domain.Exceptions.Auth;

public class UserNotFoundException : DomainException
{
    public UserNotFoundException(Guid userId) 
        : base("AUTH_USER_NOT_FOUND", $"User with ID '{userId}' was not found")
    {
    }

    public UserNotFoundException(string email) 
        : base("AUTH_USER_NOT_FOUND", $"User with email '{email}' was not found")
    {
    }
}
