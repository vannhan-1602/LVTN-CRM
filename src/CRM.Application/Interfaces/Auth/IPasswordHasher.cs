namespace CRM.Application.Interfaces.Auth;

public interface IPasswordHasher
{
    bool Verify(string password, string storedPassword);
    string Hash(string password);
}
