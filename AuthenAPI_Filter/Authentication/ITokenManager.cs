using System.Security.Claims;

namespace AuthenAPI_CustomJwt.Authentication
{
    public interface ITokenManager
    {
        bool Authenticate(string username, string password);
        string NewToken();
    }
}
