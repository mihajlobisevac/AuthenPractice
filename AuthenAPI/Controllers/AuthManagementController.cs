using AuthenAPI.Configuration;
using AuthenAPI.Data;
using AuthenAPI.Models;
using AuthenAPI.Models.Dtos.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthManagementController : ControllerBase
    {
        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly TokenValidationParameters _tokenValidationParams;
        private readonly ApiDbContext _context;

        public AuthManagementController(
            UserManager<IdentityUser> userManager, 
            IOptionsMonitor<JwtConfig> optionsMonitor,
            TokenValidationParameters tokenValidationParams,
            ApiDbContext context)
        {
            _jwtConfig = optionsMonitor.CurrentValue;
            _userManager = userManager;
            _tokenValidationParams = tokenValidationParams;
            _context = context;
        }

        public AuthResult AuthResult { get; set; } = new();

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto user)
        {
            if (await NotAvailableEmail(user.Email))
            {
                return BadRequest(AuthResult.Failed(new[] { "Email already in use." }));
            }

            var newUser = new IdentityUser { UserName = user.Username, Email = user.Email };
            var createUser = await _userManager.CreateAsync(newUser, user.Password);

            if (createUser.Succeeded)
            {
                var jwtToken = await GenerateJwtToken(newUser);
                return Ok(jwtToken);
            }

            return BadRequest(AuthResult.Failed(createUser.Errors.Select(x => x.Description).ToArray()));
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto user)
        {
            if (await NotValidEmailAndPassword(user.Email, user.Password))
            {
                return BadRequest(AuthResult.Failed(new[] { "Invalid login request." }));
            }

            var existingUser = await _userManager.FindByEmailAsync(user.Email);
            var jwtToken = await GenerateJwtToken(existingUser);

            return Ok(jwtToken);
        }

        [HttpPost]
        [Route("refreshtoken")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest tokenRequest)
        {
            var result = await VerifyAndGenerateToken(tokenRequest);

            if (result is null)
            {
                return BadRequest(AuthResult.Failed(new[] { "Invalid tokens." }));
            }

            return Ok(result);
        }

        private async Task<bool> NotAvailableEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user is not null;
        }

        private async Task<bool> NotValidEmailAndPassword(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return !await _userManager.CheckPasswordAsync(user, password);
        }

        private async Task<AuthResult> GenerateJwtToken(IdentityUser user)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(GenerateClaims(user)),
                Expires = DateTime.UtcNow.AddSeconds(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = jwtTokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = jwtTokenHandler.WriteToken(token);

            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                IsUsed = false,
                IsRevoked = false,
                UserId = user.Id,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
                Token = RandomString(35) + Guid.NewGuid()
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return AuthResult.Successful(jwtToken, refreshToken.Token);
        }

        private async Task<AuthResult> VerifyAndGenerateToken(TokenRequest tokenRequest)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Validation 1: validate jwt token format
                var tokenInVerification = jwtTokenHandler.ValidateToken(
                    tokenRequest.Token,
                    _tokenValidationParams,
                    out var validatedToken);

                // Validation 2: validate encryption algorithm
                if (validatedToken is JwtSecurityToken jwtSecurityToken)
                {
                    var result = jwtSecurityToken.Header.Alg.Equals(
                        SecurityAlgorithms.HmacSha256,
                        StringComparison.InvariantCultureIgnoreCase);

                    if (result == false)
                    {
                        return null;
                    }
                }

                // Validation 3: validate expiry date
                var utcExpiryDate = long.Parse(
                    tokenInVerification.Claims
                        .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

                var expiryDate = UnixTimestampToDateTime(utcExpiryDate);

                if (expiryDate > DateTime.UtcNow)
                {
                    return AuthResult.Failed(new[] { "Token has not yet expired." });
                }

                // Validation 4: validate existence of the token
                var storedToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(x => x.Token == tokenRequest.RefreshToken);

                if (storedToken is null)
                {
                    return AuthResult.Failed(new[] { "Token does not exist." });
                }

                // Validation 5: validate if used
                if (storedToken.IsUsed)
                {
                    return AuthResult.Failed(new[] { "Token has been used." });
                }

                // Validation 6: validate if revoked
                if (storedToken.IsRevoked)
                {
                    return AuthResult.Failed(new[] { "Token has been revoked." });
                }

                // Validation 7: validate the id
                var jti = tokenInVerification.Claims
                    .FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

                if (storedToken.JwtId != jti)
                {
                    return AuthResult.Failed(new[] { "Token does not match." });
                }

                // Update Current Token
                storedToken.IsUsed = true;
                _context.RefreshTokens.Update(storedToken);
                await _context.SaveChangesAsync();

                // Generate New Token
                var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
                return await GenerateJwtToken(dbUser);
            }
            catch (Exception ex)
            {
                var yo = ex;
                return null;
            }
        }

        private static Claim[] GenerateClaims(IdentityUser user)
        {
            return new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
        }

        private static string RandomString(int length)
        {
            var rnd = new Random();
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return new string(
                Enumerable.Repeat(chars, length)
                    .Select(x => x[rnd.Next(x.Length)])
                    .ToArray());
        }

        private static DateTime UnixTimestampToDateTime(long timestamp)
        {
            var datetimeValue = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return datetimeValue.AddSeconds(timestamp).ToUniversalTime();
        }
    }
}
