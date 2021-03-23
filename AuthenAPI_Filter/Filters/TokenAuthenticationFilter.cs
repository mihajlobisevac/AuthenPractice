using AuthenAPI_CustomFilter.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace AuthenAPI_CustomFilter.Filters
{
    public class TokenAuthenticationFilter : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var _tokenManager = (ITokenManager)context.HttpContext.RequestServices.GetService(typeof(ITokenManager));

            var result = true;

            if (!context.HttpContext.Request.Headers.ContainsKey("Authorization")) result = false;

            var token = string.Empty;

            if (result)
            {
                token = context.HttpContext.Request.Headers.First(x => x.Key == "Authorization").Value;

                if (!_tokenManager.VerifyToken(token)) result = false;
            }

            if (result == false)
            {
                context.ModelState.AddModelError("Unauthorized", "You are unauthorized.");
                context.Result = new UnauthorizedObjectResult(context.ModelState);
            }
        }
    }
}
