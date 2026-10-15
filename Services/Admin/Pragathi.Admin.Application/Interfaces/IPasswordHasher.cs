namespace Pragathi.Admin.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);                     // real password -> scrambled text
    bool Verify(string password, string storedHash); // does the typed password match?
}