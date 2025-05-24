namespace AppService.Interaces;

public interface ITokenService
{
    string GenerateToken(string username);
}