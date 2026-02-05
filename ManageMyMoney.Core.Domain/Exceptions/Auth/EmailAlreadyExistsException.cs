namespace ManageMyMoney.Core.Domain.Exceptions.Auth;

public class EmailAlreadyExistsException : DomainException
{
    public EmailAlreadyExistsException(string email) 
        : base("AUTH_EMAIL_EXISTS", $"Email '{email}' is already registered")
    {
    }
}
