﻿using System.ComponentModel.DataAnnotations;

namespace AuthenAPI.Models.Dtos.Requests
{
    public class TokenRequest
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }
}
