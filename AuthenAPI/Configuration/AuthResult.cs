using System.Collections.Generic;

namespace AuthenAPI.Configuration
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public bool Success { get; set; }
        public ICollection<string> Errors { get; set; }

        public AuthResult Successful(string jwtToken, string refreshToken)
            => new()
            {
                Token = jwtToken,
                RefreshToken = refreshToken,
                Success = true,
                Errors = null
            };

        public AuthResult Failed(string[] errors)
            => new()
            {
                Token = null,
                RefreshToken = null,
                Success = false,
                Errors = errors
            };
    }
}
