namespace RelPro.Infrastructure.Session;

public interface IHttpContextTokenExtractor
{
    string? CurrentToken { get; }
}
