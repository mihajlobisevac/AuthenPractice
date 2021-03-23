using System;

namespace AuthenAPI_CustomFilter.Authentication
{
    public class Token
    {
        public string Value { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
