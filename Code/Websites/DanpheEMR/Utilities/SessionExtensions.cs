/*
 File: SessionExtensions.cs
 created: 4Mar'17-sudarshan
 description: this class is needed to serialize session variables inside Controllers, 
             by using extension methods, you can set and get serializable objects to Session:

 remarks: check the website: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state for reference.
 -------------------------------------------------------------------
 change history:
 -------------------------------------------------------------------
 S.No     UpdatedBy/Date             description           remarks
 -------------------------------------------------------------------
 1.       sudarshan/4Mar'17          created

 -------------------------------------------------------------------
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using DanpheEMR.Security;
using DanpheEMR.Enums;

namespace DanpheEMR.Utilities
{
    public static class HttpContextHelper
    {
        private static AsyncLocal<HttpContext> _httpContext = new AsyncLocal<HttpContext>();

        public static HttpContext Current
        {
            get => _httpContext.Value;
            set => _httpContext.Value = value;
        }
    }

    public static class SessionExtensions
    {
        //adds extension/overload to Session.Set method, which by default is not available on its own.
        public static void Set<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        //adds extension/overload to Session.Get method, which by default is not available on its own
        public static T Get<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value != null)
            {
                return JsonConvert.DeserializeObject<T>(value);
            }

            // Fallback for RbacUser when session is unpopulated (e.g. stateless JWT/bearer token requests)
            if (typeof(T) == typeof(RbacUser) && string.Equals(key, "currentuser", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var httpContext = HttpContextHelper.Current;
                    if (httpContext != null)
                    {
                        string tokenFromHeader = httpContext.Request.Headers["Authorization"];
                        if (!string.IsNullOrEmpty(tokenFromHeader))
                        {
                            var tokenWithoutBearer = tokenFromHeader.Split(' ')[1];
                            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                            var jwtSecurityToken = handler.ReadJwtToken(tokenWithoutBearer);
                            var userClaim = jwtSecurityToken.Claims.Where(claim => claim.Type == ENUM_ClaimTypes.currentUser).FirstOrDefault()?.Value;
                            if (!string.IsNullOrEmpty(userClaim))
                            {
                                return (T)(object)JsonConvert.DeserializeObject<RbacUser>(userClaim);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // ignore
                }
            }

            return default(T);
        }
    }
}
