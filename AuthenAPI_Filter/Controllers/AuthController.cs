using AuthenAPI_CustomFilter.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace AuthenAPI_CustomFilter.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenManager _tokenManager;

        public AuthController(ITokenManager tokenManager)
        {
            _tokenManager = tokenManager;
        }

        [HttpGet]
        public IActionResult Authenticate(string username, string password)
        {
            if (_tokenManager.Authenticate(username, password))
            {
                return Ok(new { Token = _tokenManager.NewToken() });
            }

            ModelState.AddModelError("Unauthorized", "You are unauthorized.");
            return Unauthorized(ModelState);
        }
    }
}
