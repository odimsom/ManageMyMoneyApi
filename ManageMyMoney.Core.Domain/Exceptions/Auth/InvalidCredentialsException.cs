namespace ManageMyMoney.Core.Domain.Exceptions.Auth;

public class InvalidCredentialsException : DomainException
{
    public InvalidCredentialsException() 
        : base("AUTH_INVALID_CREDENTIALS", "Invalid email or password")
    {
    }
}
