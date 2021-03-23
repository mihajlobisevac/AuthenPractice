using System;
using System.Collections.Generic;
using System.Linq;

namespace AuthenAPI_CustomFilter.Authentication
{
    public class TokenManager : ITokenManager
    {
        private List<Token> tokenList = new();

        public bool Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) && string.IsNullOrEmpty(password)) return false;

            if (username.ToLower() != "user" && password.ToLower() != "pass") return false;

            return true;
        }

        public Token NewToken()
        {
            var token = new Token
            {
                Value = Guid.NewGuid().ToString(),
                ExpiryDate = DateTime.Now.AddSeconds(60)
            };

            tokenList.Add(token);

            return token;
        }

        public bool VerifyToken(string token)
        {
            if (tokenList.Any(x => x.Value == token && x.ExpiryDate > DateTime.Now))
            {
                return true;
            }

            return false;
        }
    }
}
