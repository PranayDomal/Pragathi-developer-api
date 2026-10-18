namespace Pragathi.Admin.Application.Exceptions;

// Thrown when the ID and password are RIGHT but the account is switched off.
public class AccountDisabledException : Exception
{
    public AccountDisabledException() : base("This account is switched off.") { }
}