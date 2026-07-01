namespace RelPro.Auth.Api.Services;

public interface IPasswordHasher
{
    bool Verify(string email, string password, string storedHash, string? storedSalt);
}
