using System;

namespace AuthenAPI_CustomJwt.Authentication
{
    public class Token
    {
        public string Value { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
