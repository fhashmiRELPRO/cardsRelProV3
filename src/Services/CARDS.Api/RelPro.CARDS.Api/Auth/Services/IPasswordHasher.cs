namespace RelPro.CARDS.Api.Auth.Services;

public interface IPasswordHasher
{
    bool Verify(string email, string password, string storedHash, string? storedSalt);
}
