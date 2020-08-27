namespace Nop.Plugin.Api.Authorization.Requirements
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using Nop.Core.Infrastructure;
    using Nop.Plugin.Api.Domain;
    using System;
    using System.Net.Http.Headers;

    public class BasicAuthenticationRequirement : IAuthorizationRequirement
    {
        public bool HasAccess(HttpContext httpContext)
        {
            var req = httpContext.Request;

            if (req.Headers.TryGetValue("Authorization", out StringValues auth))
            {
                var authHeader = AuthenticationHeaderValue.Parse(auth);

                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(authHeader.Parameter)).Split(':');
                var user = new { ClientId = cred[0], ClientSecret = cred[1] };

                var settings = EngineContext.Current.Resolve<ApiSettings>();
                if (settings.ClientId == user.ClientId && settings.ClientSecret == user.ClientSecret)
                {
                    return true;
                }
            }

            return false;
        }
    }
}