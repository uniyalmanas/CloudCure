using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DanpheEMR.Security;
using DanpheEMR.Utilities;
using DanpheEMR.Enums;

namespace DanpheEMR.Utilities
{
    public class SessionTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            try
            {
                // Check if session doesn't have currentuser but we have Authorization header
                if (httpContext.Session.Get<RbacUser>("currentuser") == null)
                {
                    string tokenFromHeader = httpContext.Request.Headers["Authorization"];
                    if (!string.IsNullOrEmpty(tokenFromHeader))
                    {
                        string tokenWithoutBearer = null;
                        if (tokenFromHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                        {
                            tokenWithoutBearer = tokenFromHeader.Substring(7);
                        }
                        else
                        {
                            tokenWithoutBearer = tokenFromHeader;
                        }

                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        if (handler.CanReadToken(tokenWithoutBearer))
                        {
                            var jwtSecurityToken = handler.ReadJwtToken(tokenWithoutBearer);
                            var userClaim = jwtSecurityToken.Claims.FirstOrDefault(claim => claim.Type == ENUM_ClaimTypes.currentUser)?.Value;
                            if (!string.IsNullOrEmpty(userClaim))
                            {
                                var user = DanpheJSONConvert.DeserializeObject<RbacUser>(userClaim);
                                if (user != null)
                                {
                                    httpContext.Session.Set<RbacUser>("currentuser", user);
                                    httpContext.Session.Set<RbacUser>(ENUM_SessionVariables.CurrentUser, user);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Safe fall-through
            }

            await _next(httpContext);
        }
    }
}
